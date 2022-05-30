using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Dunet;

[Generator]
public class DiscriminatedUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "UnionAttribute.g.cs",
            SourceText.From(UnionSource.Attribute, Encoding.UTF8))
        );

        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsInterface(node),
                transform: static (ctx, _) => GetGenerationTarget(ctx))
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

        context.RegisterSourceOutput(compilation,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsInterface(SyntaxNode node) =>
        node is InterfaceDeclarationSyntax interfaceDeclaration
        && interfaceDeclaration.AttributeLists.Count > 0;

    private static InterfaceDeclarationSyntax? GetGenerationTarget(GeneratorSyntaxContext context)
    {
        var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

        foreach (var attributeList in interfaceDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var interfaceSymbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol?.ContainingType;

                if (interfaceSymbol is null)
                {
                    continue;
                }

                var fullInterfaceName = interfaceSymbol.ToDisplayString();

                if (fullInterfaceName is UnionSource.FullAttributeName)
                {
                    return interfaceDeclaration;
                }
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax> interfaces,
        SourceProductionContext context)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctInterfaces = interfaces.Distinct();

        var recordsToGenerate = GetRecordsToGenerate(compilation, distinctInterfaces, context.CancellationToken);

        if (recordsToGenerate.Count <= 0)
        {
            return;
        }

        foreach (var recordToGenerate in recordsToGenerate)
        {
            var result = UnionSource.GenerateRecord(recordToGenerate);
            context.AddSource($"{recordToGenerate.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static List<RecordToGenerate> GetRecordsToGenerate(
        Compilation compilation,
        IEnumerable<InterfaceDeclarationSyntax> interfaces,
        CancellationToken cancellationToken)
    {
        var interfaceMethods = new List<Method>()
        {
            new Method("Circle", new()
            {
                new Parameter("double", "radius"),
            }),
            new Method("Rectangle", new()
            {
                new Parameter("double", "length"),
                new Parameter("double", "width"),
            }),
            new Method("Triangle", new()
            {
                new Parameter("double", "@base"),
                new Parameter("double", "height"),
            }),
        };

        return new()
        {
            new RecordToGenerate(
                Namespace: "Dunet.Cli",
                Name: "Rectangle",
                Interface: "IShape",
                Properties: new()
                {
                    new Parameter("double", "Length"),
                    new Parameter("double", "Width"),
                },
                Methods: interfaceMethods
            ),
            new RecordToGenerate(
                Namespace: "Dunet.Cli",
                Name: "Triangle",
                Interface: "IShape",
                Properties: new()
                {
                    new Parameter("double", "Base"),
                    new Parameter("double", "Height"),
                },
                Methods: interfaceMethods
            ),
            new RecordToGenerate(
                Namespace: "Dunet.Cli",
                Name: "Circle",
                Interface: "IShape",
                Properties: new()
                {
                    new Parameter("double", "Radius"),
                },
                Methods: interfaceMethods
            ),
        };
    }
}