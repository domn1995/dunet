using Microsoft.CodeAnalysis;

namespace Dunet.GenerateUnionRecord;

internal sealed record UnionDeclaration(
    List<string> Imports,
    string? Namespace,
    Accessibility Accessibility,
    string Name,
    List<TypeParameter> TypeParameters,
    List<TypeParameterConstraint> TypeParameterConstraints,
    List<VariantDeclaration> Variants,
    Stack<ParentType> ParentTypes,
    List<Property> Properties
)
{
    // Extension methods cannot be generated for a union declared in a top level program (no namespace).
    // It also doesn't make sense to generate Match extensions if there are no variants to match aginst.
    public bool SupportsAsyncMatchExtensionMethods() => Namespace is not null && Variants.Count > 0;

    public bool SupportsImplicitConversions()
    {
        var allVariantsHaveSingleProperty = () =>
            Variants.All(static variant => variant.Parameters.Count is 1);

        var allVariantsHaveNoInterfaceParameters = () =>
            Variants
                .SelectMany(static variant => variant.Parameters)
                .All(static property => !property.Type.IsInterface);

        var allVariantsHaveUniquePropertyTypes = () =>
        {
            var allPropertyTypes = Variants
                .SelectMany(static variant => variant.Parameters)
                .Select(static property => property.Type);
            var allPropertyTypesCount = allPropertyTypes.Count();
            var uniquePropertyTypesCount = allPropertyTypes.Distinct().Count();
            return allPropertyTypesCount == uniquePropertyTypesCount;
        };

        var hasNoRequiredProperties = () => !Properties.Any(property => property.IsRequired);

        return allVariantsHaveSingleProperty()
            && allVariantsHaveNoInterfaceParameters()
            && allVariantsHaveUniquePropertyTypes()
            && hasNoRequiredProperties();
    }
}

internal sealed record VariantDeclaration
{
    public required string Identifier { get; init; }
    public required List<TypeParameter> TypeParameters { get; init; }
    public required List<Parameter> Parameters { get; init; }
}

internal sealed record TypeParameter(string Identifier)
{
    public override string ToString() => Identifier;
}

internal sealed record Parameter(ParameterType Type, string Identifier);

internal sealed record Property(PropertyType Type, string Identifier, bool IsRequired);

internal sealed record ParameterType(string Identifier, bool IsInterface);

internal sealed record PropertyType(string Identifier, bool IsInterface);

/// <summary>
/// Represents a parent type declaration that nests a union record.
/// </summary>
/// <param name="IsRecord">Whether the type is a record or a plain class.</param>
/// <param name="Identifier">This type's name.</param>
internal sealed record ParentType(bool IsRecord, string Identifier)
{
    public override string ToString() => Identifier;
}

/// <summary>
/// Represents a type parameter constraint on a union record's type parameters.
/// </summary>
/// <param name="Value">The full string value of the constraint.<br />
/// Ex: <c>where T : notnull, System.Exception</c></param>
internal sealed record TypeParameterConstraint(string Value)
{
    public override string ToString() => Value;
}
