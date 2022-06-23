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
            .Where(node => node.IsKind(SyntaxKind.MethodDeclaration))
            .OfType<MethodDeclarationSyntax>();

    public static string? GetNamespace(this INamedTypeSymbol symbol) =>
        symbol.ContainingNamespace.ToString() is "<global namespace>"
            ? null
            : symbol.ContainingNamespace.ToString();
}
