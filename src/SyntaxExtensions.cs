using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet;

internal static class SyntaxExtensions
{
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

    public static bool IsDecoratedRecord(this SyntaxNode node) =>
        node is RecordDeclarationSyntax recordDeclaration
        && recordDeclaration.AttributeLists.Count > 0;

    public static bool IsImporting(this UsingDirectiveSyntax import, string name) =>
        import.Name.ToString() == name;
}
