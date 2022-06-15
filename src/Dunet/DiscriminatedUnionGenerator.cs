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
        context.RegisterPostInitializationOutput(
            ctx =>
                ctx.AddSource(
                    "UnionAttribute.g.cs",
                    SourceText.From(UnionSource.Attribute, Encoding.UTF8)
                )
        );

        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsInterface(node),
                transform: static (ctx, _) => GetGenerationTarget(ctx)
            )
            .Where(static m => m is not null);

        var compilation = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

        context.RegisterSourceOutput(
            compilation,
            static (spc, source) => Execute(source.Left, source.Right!, spc)
        );
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
                var interfaceSymbol = context.SemanticModel
                    .GetSymbolInfo(attribute)
                    .Symbol?.ContainingType;

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
        SourceProductionContext context
    )
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctInterfaces = interfaces.Distinct();

        var recordsToGenerate = GetRecordsToGenerate(
            compilation,
            distinctInterfaces,
            context.CancellationToken
        );

        if (recordsToGenerate.Count <= 0)
        {
            return;
        }

        foreach (var recordToGenerate in recordsToGenerate)
        {
            var result = UnionSource.GenerateRecord(recordToGenerate);
            context.AddSource(
                $"{recordToGenerate.Name}.g.cs",
                SourceText.From(result, Encoding.UTF8)
            );
        }
    }

    private static List<RecordToGenerate> GetRecordsToGenerate(
        Compilation compilation,
        IEnumerable<InterfaceDeclarationSyntax> interfaces,
        CancellationToken cancellationToken
    )
    {
        var recordsToGenerate = new List<RecordToGenerate>();
        foreach (var iface in interfaces)
        {
            var interfaceMethods = new List<Method>();
            var methodDeclarations = iface
                .DescendantNodes()
                .Where(node => node.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>()
                .ToList();

            foreach (var methodDeclaration in methodDeclarations)
            {
                var methodReturnType = methodDeclaration.ReturnType.ToString();
                var methodName = methodDeclaration.Identifier.ToString();
                var methodParams = methodDeclaration.ParameterList.Parameters;
                var parameters = new List<Parameter>();
                foreach (var methodParam in methodParams)
                {
                    var name = methodParam.Identifier.ToString();
                    var parameter = new Parameter(Type: methodParam.Type!.ToString(), Name: name);
                    parameters.Add(parameter);
                }
                var method = new Method(methodReturnType, methodName, parameters);
                interfaceMethods.Add(method);
            }

            var semanticModel = compilation.GetSemanticModel(iface.SyntaxTree);
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(iface);
            if (interfaceSymbol is null)
            {
                continue;
            }
            var containingNamespace = interfaceSymbol.ContainingNamespace.ToString();
            var @namespace = containingNamespace is "<global namespace>"
                ? null
                : containingNamespace;

            foreach (var interfaceMethod in interfaceMethods)
            {
                var name = interfaceMethod.Name;
                var recordProperties = interfaceMethod.Parameters
                    .Select(
                        param => new Parameter(Type: param.Type, Name: param.Name.ToPropertyCase())
                    )
                    .ToList();
                var recordToGenerate = new RecordToGenerate(
                    Namespace: @namespace,
                    Name: name,
                    Interface: interfaceSymbol.Name,
                    Properties: new(recordProperties),
                    Methods: interfaceMethods
                );
                recordsToGenerate.Add(recordToGenerate);
            }
        }

        return recordsToGenerate;
    }
}
