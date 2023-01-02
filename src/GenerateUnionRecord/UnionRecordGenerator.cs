using Dunet.GenerateUnionExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Dunet.GenerateUnionRecord;

[Generator]
public sealed class UnionRecordGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node.IsDecoratedRecord(),
                transform: static (ctx, _) => GetGenerationTarget(ctx)
            )
            .Flatten()
            .Collect();

        var compilation = context.CompilationProvider.Combine(targets);

        context.RegisterSourceOutput(
            compilation,
            static (spc, source) => Execute(source.Left, source.Right, spc)
        );
    }

    private static RecordDeclarationSyntax? GetGenerationTarget(GeneratorSyntaxContext context) =>
        context.Node is RecordDeclarationSyntax record
        && record.IsDecoratedWithUnionAttribute(context.SemanticModel)
            ? record
            : null;

    private static void Execute(
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax> recordDeclarations,
        SourceProductionContext context
    )
    {
        if (recordDeclarations.IsDefaultOrEmpty)
        {
            return;
        }

        var unionRecords = GetCodeToGenerate(
            compilation,
            recordDeclarations,
            context.CancellationToken
        );

        foreach (var unionRecord in unionRecords)
        {
            var union = UnionRecordSource.GenerateRecord(unionRecord);
            context.AddSource(
                $"{unionRecord.Namespace}.{unionRecord.Name}.g.cs",
                SourceText.From(union, Encoding.UTF8)
            );

            if (unionRecord.SupportsAsyncMatchExtensionMethods())
            {
                var matchExtensions = UnionExtensionsSource.GenerateExtensions(unionRecord);
                context.AddSource(
                    $"{unionRecord.Namespace}.{unionRecord.Name}MatchExtensions.g.cs",
                    SourceText.From(matchExtensions, Encoding.UTF8)
                );
            }
        }
    }

    private static List<UnionRecord> GetCodeToGenerate(
        Compilation compilation,
        IEnumerable<RecordDeclarationSyntax> declarations,
        CancellationToken _
    ) =>
        declarations
            .Select(declaration =>
            {
                var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
                var recordSymbol = semanticModel.GetDeclaredSymbol(declaration);

                if (recordSymbol is null)
                {
                    return null;
                }

                var imports = declaration
                    .GetImports()
                    .Where(static usingDirective => !usingDirective.IsImporting("Dunet"))
                    .Select(static usingDirective => usingDirective.ToString());
                var @namespace = recordSymbol.GetNamespace();
                var typeParameters = declaration.GetTypeParameters();
                var typeParameterConstraints = declaration.GetTypeParameterConstraints();
                var unionRecordMembers = declaration.GetNestedRecordDeclarations(semanticModel);
                var parentTypes = declaration.GetParentTypes(semanticModel);

                return new UnionRecord(
                    Imports: imports.ToList(),
                    Namespace: @namespace,
                    Accessibility: recordSymbol.DeclaredAccessibility,
                    Name: recordSymbol.Name,
                    TypeParameters: typeParameters?.ToList() ?? new(),
                    TypeParameterConstraints: typeParameterConstraints?.ToList() ?? new(),
                    Members: unionRecordMembers.ToList(),
                    ParentTypes: parentTypes
                );
            })
            .OfType<UnionRecord>()
            .ToList();
}
