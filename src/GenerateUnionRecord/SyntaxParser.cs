using Dunet.UnionAttributeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet.GenerateUnionRecord;

/// <summary>
/// Retrieves semantic information from record declarations.
/// </summary>
internal static class RecordDeclarationSyntaxParser
{
    /// <summary>
    /// Gets the type parameters of this record declaration.
    /// </summary>
    /// <param name="record">This record declaration.</param>
    /// <returns>The sequence of type parameters, if any. Otherwise, <c>null</c>.</returns>
    public static IEnumerable<TypeParameter>? GetTypeParameters(
        this RecordDeclarationSyntax record
    ) =>
        record.TypeParameterList?.Parameters.Select(
            static typeParam => new TypeParameter(typeParam.Identifier.ToString())
        );

    /// <summary>
    /// Gets the type parameter constraints of this record declaration.
    /// </summary>
    /// <param name="record">This record declaration.</param>
    /// <returns>The sequence of type parameter constraints, if any. Otherwise, <c>null</c>.</returns>
    public static IEnumerable<TypeParameterConstraint>? GetTypeParameterConstraints(
        this RecordDeclarationSyntax record
    ) =>
        record.ConstraintClauses.Select(
            static constraint => new TypeParameterConstraint(constraint.ToString())
        );

    /// <summary>
    /// Gets the properties within this record declaration.
    /// </summary>
    /// <param name="record">This record declaration.</param>
    /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
    /// <returns>The sequence of properties, if any. Otherwise, <c>null</c>.</returns>
    public static IEnumerable<Property>? GetProperties(
        this RecordDeclarationSyntax record,
        SemanticModel semanticModel
    ) =>
        record.ParameterList?.Parameters.Select(
            parameter =>
                new Property(
                    Type: new PropertyType(
                        Identifier: parameter.Type?.ToString() ?? "",
                        IsInterface: parameter.Type.IsInterfaceType(semanticModel)
                    ),
                    Identifier: parameter.Identifier.ToString()
                )
        );

    /// <summary>
    /// Gets the record declarations within this record declaration.
    /// </summary>
    /// <param name="record">This record declaration.</param>
    /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
    /// <returns>The sequence of nested record declarations, if any. Otherwise, <c>null</c>.</returns>
    public static IEnumerable<UnionRecordMember> GetNestedRecordDeclarations(
        this RecordDeclarationSyntax record,
        SemanticModel semanticModel
    ) =>
        record
            .DescendantNodes()
            .Where(static node => node.IsKind(SyntaxKind.RecordDeclaration))
            .OfType<RecordDeclarationSyntax>()
            .Select(
                member =>
                    new UnionRecordMember()
                    {
                        Identifier = member.Identifier.ToString(),
                        TypeParameters = member.GetTypeParameters()?.ToList() ?? new(),
                        Properties = member.GetProperties(semanticModel)?.ToList() ?? new()
                    }
            );

    /// <summary>
    /// Determines whether this record declaration is decorated by this library's marker attribute:
    /// <see cref="UnionAttributeSource.FullyQualifiedName"/>.
    /// </summary>
    /// <param name="record">This record declaration.</param>
    /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
    /// <returns>
    /// A boolean <c>true</c> if decorated with the union attribute; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDecoratedWithUnionAttribute(
        this RecordDeclarationSyntax record,
        SemanticModel semanticModel
    )
    {
        var getDecoratedType = (AttributeSyntax attributeSyntax) =>
            semanticModel.GetSymbolInfo(attributeSyntax).Symbol?.ContainingType;

        return record.AttributeLists
            .SelectMany(static attributeListSyntax => attributeListSyntax.Attributes)
            .Select(getDecoratedType)
            .Select(static attributeSymbol => attributeSymbol?.ToDisplayString())
            .Any(static attributeName => attributeName is UnionAttributeSource.FullyQualifiedName);
    }
}
