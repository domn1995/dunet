using System.Collections.Immutable;
using System.Text;
using Dunet.Generator.UnionExtensionsGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dunet.Generator.UnionGeneration;

[Generator]
public sealed class UnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node.IsDecoratedRecord(),
                transform: static (ctx, _) => GetGenerationTarget(ctx)
            )
            .Flatten()
            .Collect();

        var compilation = context.CompilationProvider.Combine(targets);

        var parsedModel = compilation.Select(static (x, token) => Parse(x.Left, x.Right, token));
        var splitModel = parsedModel.SelectMany(static (result, _) => result);

        context.RegisterSourceOutput(splitModel, Emit);
    }

    private static RecordDeclarationSyntax? GetGenerationTarget(GeneratorSyntaxContext context) =>
        context.Node is RecordDeclarationSyntax record
        && record.IsDecoratedWithUnionAttribute(context.SemanticModel)
            ? record
            : null;

    private static void Emit(SourceProductionContext context, UnionDeclaration unionRecord)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        var union = UnionSourceBuilder.Build(unionRecord);
        context.AddSource(
            $"{unionRecord.Namespace}.{unionRecord.Name}.g.cs",
            SourceText.From(union, Encoding.UTF8)
        );

        if (context.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (unionRecord.SupportsExtensionMethods())
        {
            var matchExtensions = UnionExtensionsSourceBuilder.GenerateExtensions(unionRecord);
            context.AddSource(
                $"{unionRecord.Namespace}.{unionRecord.Name}MatchExtensions.g.cs",
                SourceText.From(matchExtensions, Encoding.UTF8)
            );
        }
    }

    private static IEnumerable<UnionDeclaration> Parse(
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax> declarations,
        CancellationToken cancellation
    )
    {
        foreach (var declaration in declarations)
        {
            if (cancellation.IsCancellationRequested)
            {
                yield break;
            }

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            var recordSymbol = semanticModel.GetDeclaredSymbol(declaration);

            if (recordSymbol is null)
            {
                continue;
            }

            var imports = declaration
                .GetImports()
                .Where(static usingDirective => !usingDirective.IsImporting("Dunet"))
                .Select(static usingDirective => usingDirective.ToString());
            var @namespace = recordSymbol.GetNamespace();
            var typeParameters = declaration.GetTypeParameters();
            var typeParameterConstraints = declaration.GetTypeParameterConstraints();
            var variants = declaration.GetNestedRecordDeclarations(semanticModel);
            var parentTypes = declaration.GetParentTypes(semanticModel);
            var properties = declaration.GetProperties(semanticModel);

            yield return new UnionDeclaration(
                Imports: imports.ToImmutableEquatableArray(),
                Namespace: @namespace,
                Accessibility: recordSymbol.DeclaredAccessibility,
                Name: recordSymbol.Name,
                TypeParameters: typeParameters.ToImmutableEquatableArray(),
                TypeParameterConstraints: typeParameterConstraints.ToImmutableEquatableArray(),
                Variants: variants.ToImmutableEquatableArray(),
                ParentTypes: parentTypes.ToImmutableEquatableArray(),
                Properties: properties.ToImmutableEquatableArray()
            );
        }
    }
}
