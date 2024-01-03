using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet.Generator;

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
        node is RecordDeclarationSyntax { AttributeLists.Count: > 0 };

    public static bool IsImporting(this UsingDirectiveSyntax import, string name) =>
        import.Name?.ToString() == name;

    public static bool IsPartial(this TypeDeclarationSyntax declaration) =>
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static bool IsRecordDeclaration(this SyntaxNode node) => node is RecordDeclarationSyntax;

    public static bool IsClassDeclaration(this SyntaxNode node) => node is ClassDeclarationSyntax;

    public static bool IsClassOrRecordDeclaration(this SyntaxNode node) =>
        node.IsRecordDeclaration() || node.IsClassDeclaration();

    public static string ToKeyword(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.ProtectedOrInternal
            or Accessibility.ProtectedOrFriend
                => "protected internal",
            Accessibility.Internal or Accessibility.Friend => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal
            or Accessibility.ProtectedAndFriend
                => "private protected",
            Accessibility.Private => "private",
            Accessibility.NotApplicable => "",
            _ => "",
        };

    public static bool IsInterfaceType(this TypeSyntax? typeSyntax, SemanticModel semanticModel) =>
        typeSyntax is not null
        && semanticModel.GetTypeInfo(typeSyntax).Type?.TypeKind is TypeKind.Interface;
}
