using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet;

public static class SyntaxExtensions
{
    public static IEnumerable<MethodDeclarationSyntax> GetMethodDeclarations(
        this TypeDeclarationSyntax typeDeclaration
    ) =>
        typeDeclaration
            .DescendantNodes()
            .Where(static node => node.IsKind(SyntaxKind.MethodDeclaration))
            .OfType<MethodDeclarationSyntax>();

    public static string? GetNamespace(this INamedTypeSymbol symbol) =>
        symbol.ContainingNamespace.ToString() switch
        {
            "<global namespace>" => null,
            var ns => ns,
        };

    public static IEnumerable<UsingDirectiveSyntax> GetImports(
        this TypeDeclarationSyntax typeDeclaration
    ) =>
        typeDeclaration.SyntaxTree.GetRoot() switch
        {
            CompilationUnitSyntax root => root.Usings,
            _ => Enumerable.Empty<UsingDirectiveSyntax>(),
        };

    public static bool IsImporting(this UsingDirectiveSyntax import, string name) =>
        import.Name.ToString() == name;
}
