using Dunet.GenerateUnionExtensions;
using Dunet.UnionAttributeGeneration;
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
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider.Combine(targets.Collect());

        context.RegisterSourceOutput(
            compilation,
            static (spc, source) => Execute(source.Left, source.Right!, spc)
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

                return new UnionRecord(
                    Imports: imports.ToList(),
                    Namespace: @namespace,
                    Accessibility: recordSymbol.DeclaredAccessibility,
                    Name: recordSymbol.Name,
                    TypeParameters: typeParameters?.ToList() ?? new(),
                    TypeParameterConstraints: typeParameterConstraints?.ToList() ?? new(),
                    Members: unionRecordMembers.ToList(),
                    ParentTypes: GetParentTypes(semanticModel, declaration)
                );
            })
            .OfType<UnionRecord>()
            .ToList();

    private static Stack<ParentType> GetParentTypes(
        SemanticModel semanticModel,
        RecordDeclarationSyntax recordDeclaration
    )
    {
        var parentTypes = new Stack<ParentType>();

        RecursivelyAddParentTypes(semanticModel, recordDeclaration, parentTypes);

        return parentTypes;
    }

    private static void RecursivelyAddParentTypes(
        SemanticModel semanticModel,
        SyntaxNode declaration,
        Stack<ParentType> parentTypes
    )
    {
        var parent = declaration.Parent;

        if (parent is null)
        {
            return;
        }

        if (!parent.IsClassOrRecordDeclaration())
        {
            return;
        }

        var parentSymbol = semanticModel.GetDeclaredSymbol(parent);

        // Ignore top level statement synthentic program class.
        if (parentSymbol?.ToDisplayString() is null or "<top-level-statements-entry-point>")
        {
            return;
        }

        var parentDeclaration = (TypeDeclarationSyntax)parent;

        // We can only declare a nested union type within a partial parent type declaration.
        if (!parentDeclaration.IsPartial())
        {
            return;
        }

        var parentType = new ParentType(
            IsRecord: parent.IsRecordDeclaration(),
            Name: parentSymbol.Name
        );

        parentTypes.Push(parentType);

        RecursivelyAddParentTypes(semanticModel, parent, parentTypes);
    }
}
