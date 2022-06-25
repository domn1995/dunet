using Dunet.UnionInterface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Dunet;

[Generator]
public class UnionInterfaceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            ctx =>
                ctx.AddSource(
                    "UnionAttribute.g.cs",
                    SourceText.From(UnionInterfaceSource.Attribute, Encoding.UTF8)
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

                if (fullInterfaceName is UnionInterfaceSource.FullAttributeName)
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

        var (recordsToGenerate, matchMethodsToGenerate) = GetCodeToGenerate(
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
            var result = UnionInterfaceSource.GenerateRecord(recordToGenerate);
            context.AddSource(
                $"{recordToGenerate.Name}.g.cs",
                SourceText.From(result, Encoding.UTF8)
            );
        }

        foreach (var matchMethodToGenerate in matchMethodsToGenerate)
        {
            var result = UnionInterfaceSource.GenerateMatchMethod(matchMethodToGenerate);
            context.AddSource(
                $"{matchMethodToGenerate.Interface}DiscriminatedUnionExtensions.g.cs",
                SourceText.From(result, Encoding.UTF8)
            );
        }
    }

    private static CodeToGenerate GetCodeToGenerate(
        Compilation compilation,
        IEnumerable<InterfaceDeclarationSyntax> interfaces,
        CancellationToken _
    )
    {
        var recordsToGenerate = new List<RecordToGenerate>();
        var matchMethodsToGenerate = new List<MatchMethodToGenerate>();

        foreach (var iface in interfaces)
        {
            var semanticModel = compilation.GetSemanticModel(iface.SyntaxTree);
            var interfaceSymbol = semanticModel.GetDeclaredSymbol(iface);
            var imports = iface
                .GetImports()
                .Where(static import => !import.IsImporting("Dunet"))
                .Select(static import => import.ToString())
                .ToList();

            if (interfaceSymbol is null)
            {
                continue;
            }

            var interfaceMethods = GetInterfaceMethods(iface).ToList();

            var @namespace = interfaceSymbol.GetNamespace();
            var matchMethodParameters = new List<MatchMethodParameter>();

            foreach (var interfaceMethod in interfaceMethods)
            {
                var recordProperties = interfaceMethod.Parameters
                    .Select(static param => new Parameter(param.Type, param.Name.ToPropertyCase()))
                    .ToList();
                var recordToGenerate = new RecordToGenerate(
                    Imports: imports,
                    Namespace: @namespace,
                    Name: interfaceMethod.Name,
                    Interface: interfaceSymbol.Name,
                    Properties: new(recordProperties),
                    Methods: interfaceMethods
                );
                recordsToGenerate.Add(recordToGenerate);

                var matchMethodParameter = new MatchMethodParameter(Type: recordToGenerate.Name);
                matchMethodParameters.Add(matchMethodParameter);
            }

            var matchMethodToGenerate = new MatchMethodToGenerate(
                Imports: imports,
                Accessibility: interfaceSymbol.DeclaredAccessibility,
                Namespace: @namespace,
                Interface: interfaceSymbol.Name,
                Parameters: matchMethodParameters
            );

            matchMethodsToGenerate.Add(matchMethodToGenerate);
        }

        return new(Records: recordsToGenerate, Methods: matchMethodsToGenerate);
    }

    private static IEnumerable<Method> GetInterfaceMethods(
        InterfaceDeclarationSyntax interfaceDeclaration
    )
    {
        foreach (var methodDeclaration in interfaceDeclaration.GetMethodDeclarations())
        {
            var methodParams = methodDeclaration.ParameterList.Parameters;
            var parameters = methodParams.Select(
                static param => new Parameter(param.Type!.ToString(), param.Identifier.ToString())
            );
            var methodReturnType = methodDeclaration.ReturnType.ToString();
            var methodName = methodDeclaration.Identifier.ToString();
            var method = new Method(methodReturnType, methodName, parameters.ToList());
            yield return method;
        }
    }
}

record CodeToGenerate(List<RecordToGenerate> Records, List<MatchMethodToGenerate> Methods);
