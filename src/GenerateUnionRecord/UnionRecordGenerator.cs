using Dunet.UnionAttributeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dunet.GenerateUnionRecord;

record GenerationTarget(RecordDeclarationSyntax RecordDeclaration,
    IEnumerable<AttributeArgumentSyntax> AttributeArguments);

[Generator]
public class UnionRecordGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var recordDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node.IsDecoratedRecord(),
                transform: static (ctx, _) => GetGenerationTarget(ctx)
            )
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider
            .Combine(recordDeclarations.Collect())
            .Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(
            compilation,
            static (spc, xy) => Execute(xy.Left.Left, xy.Left.Right!, xy.Right, spc)
        );
    }

    private static GenerationTarget? GetGenerationTarget(GeneratorSyntaxContext context)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;

        foreach (var attributeList in recordDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeSymbol = context.SemanticModel
                    .GetSymbolInfo(attribute)
                    .Symbol?.ContainingType;

                if (attributeSymbol is null)
                {
                    continue;
                }

                var fullAttributeName = attributeSymbol.ToDisplayString();

                if (fullAttributeName is UnionAttributeSource.FullAttributeName)
                {
                    return new GenerationTarget(recordDeclaration,
                        attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>());
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation,
        ImmutableArray<GenerationTarget> targets,
        AnalyzerConfigOptionsProvider configOptions,
        SourceProductionContext context)
    {
        if (targets.IsDefaultOrEmpty)
        {
            return;
        }
        
        UnionAttributeOptions defaultUnionOptions;
        try
        {
            defaultUnionOptions = UnionAttributeOptions.CreateDefault(configOptions);
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("DU1001", "Dunet configuration error", "Invalid Dunet configuration: {0}", "Configuration",
                    DiagnosticSeverity.Error, true), Location.None, e.Message));
            return;
        }
        
        context.AddSource(
            $"{UnionAttributeSource.AttributeName}.Properties.g.cs",
            SourceText.From(UnionAttributeSource.GetPropertySource(defaultUnionOptions), Encoding.UTF8)
        );
        
        var unionRecords = GetCodeToGenerate(
            compilation,
            targets,
            defaultUnionOptions,
            context.CancellationToken
        );

        if (unionRecords.Count <= 0)
        {
            return;
        }

        foreach (var unionRecord in unionRecords)
        {
            var result = UnionRecordSource.GenerateRecord(unionRecord);
            context.AddSource($"{unionRecord.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static List<UnionRecord> GetCodeToGenerate(Compilation compilation,
        IEnumerable<GenerationTarget> targets,
        UnionAttributeOptions defaultUnionOptions,
        CancellationToken _)
    {
        var unionRecords = new List<UnionRecord>();

        foreach (var target in targets)
        {
            var semanticModel = compilation.GetSemanticModel(target.RecordDeclaration.SyntaxTree);
            var recordSymbol = semanticModel.GetDeclaredSymbol(target.RecordDeclaration);

            if (recordSymbol is null)
            {
                continue;
            }

            var imports = target.RecordDeclaration
                .GetImports()
                .Where(static import => !import.IsImporting("Dunet"))
                .Select(static import => import.ToString());
            var @namespace = recordSymbol.GetNamespace();
            var unionRecordTypeParameters = target.RecordDeclaration.TypeParameterList?.Parameters.Select(
                static typeParam => new TypeParameter(typeParam.Identifier.ToString())
            );
            var unionRecordMemberDeclarations = target.RecordDeclaration
                .DescendantNodes()
                .Where(static node => node.IsKind(SyntaxKind.RecordDeclaration))
                .OfType<RecordDeclarationSyntax>();

            var unionRecordMembers = new List<UnionRecordMember>();

            foreach (var memberRecordDeclaration in unionRecordMemberDeclarations)
            {
                var typeParameters = memberRecordDeclaration.TypeParameterList?.Parameters
                    .Select(static typeParam => typeParam.Identifier.ToString())
                    .Select(static identifier => new TypeParameter(identifier));
                var properties = memberRecordDeclaration.ParameterList?.Parameters.Select(
                    static parameter =>
                        new RecordProperty(
                            Type: parameter.Type?.ToString() ?? "",
                            Name: parameter.Identifier.ToString()
                        )
                );
                var memberRecord = new UnionRecordMember(
                    Name: memberRecordDeclaration.Identifier.ToString(),
                    TypeParameters: typeParameters?.ToList() ?? new(),
                    Properties: properties?.ToList() ?? new()
                );

                unionRecordMembers.Add(memberRecord);
            }

            var options = target.AttributeArguments.Aggregate(defaultUnionOptions,
                (options, argument) => options.WithAttributeArgument(semanticModel, argument));

            var record = new UnionRecord(
                Imports: imports.ToList(),
                Namespace: @namespace,
                Name: recordSymbol.Name,
                TypeParameters: unionRecordTypeParameters?.ToList() ?? new(),
                Members: unionRecordMembers,
                Options: options 
            );

            unionRecords.Add(record);
        }

        return unionRecords;
    }
}
