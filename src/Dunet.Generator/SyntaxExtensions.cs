using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet.Generator;

internal static class SyntaxExtensions
{
    extension(INamedTypeSymbol self)
    {
        public string? GetNamespace() =>
            self.ContainingNamespace.ToString() switch
            {
                "<global namespace>" => null,
                var ns => ns,
            };
    }

    extension(TypeDeclarationSyntax self)
    {
        public IEnumerable<UsingDirectiveSyntax> GetImports() =>
            self.SyntaxTree.GetRoot() switch
            {
                CompilationUnitSyntax root => root.Usings,
                _ => Enumerable.Empty<UsingDirectiveSyntax>(),
            };

        public bool IsPartial() => self.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    extension(SyntaxNode self)
    {
        public bool IsDecoratedRecord() =>
            self is RecordDeclarationSyntax { AttributeLists.Count: > 0 };

        public bool IsRecordDeclaration() => self is RecordDeclarationSyntax;

        public bool IsClassDeclaration() => self is ClassDeclarationSyntax;

        public bool IsClassOrRecordDeclaration() =>
            self.IsRecordDeclaration() || self.IsClassDeclaration();
    }

    extension(UsingDirectiveSyntax self)
    {
        public bool IsImporting(string name) => self.Name?.ToString() == name;
    }

    extension(Accessibility self)
    {
        public string ToKeyword() =>
            self switch
            {
                Accessibility.Public => "public",
                Accessibility.ProtectedOrInternal or Accessibility.ProtectedOrFriend =>
                    "protected internal",
                Accessibility.Internal or Accessibility.Friend => "internal",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedAndInternal or Accessibility.ProtectedAndFriend =>
                    "private protected",
                Accessibility.Private => "private",
                Accessibility.NotApplicable => "",
                _ => "",
            };
    }

    extension(TypeSyntax? self)
    {
        public bool IsInterfaceType(SemanticModel semanticModel) =>
            self is not null
            && semanticModel.GetTypeInfo(self).Type?.TypeKind is TypeKind.Interface;
    }
}
