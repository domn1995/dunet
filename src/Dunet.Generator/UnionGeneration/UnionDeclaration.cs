using Microsoft.CodeAnalysis;

namespace Dunet.Generator.UnionGeneration;

internal sealed record UnionDeclaration(
    ImmutableEquatableArray<string> Imports,
    string? Namespace,
    Accessibility Accessibility,
    string Name,
    ImmutableEquatableArray<TypeParameter> TypeParameters,
    ImmutableEquatableArray<TypeParameterConstraint> TypeParameterConstraints,
    ImmutableEquatableArray<VariantDeclaration> Variants,
    ImmutableEquatableArray<ParentType> ParentTypes,
    ImmutableEquatableArray<Property> Properties
)
{
    // Extension methods cannot be generated for a union declared in a top level program (no namespace).
    // It also doesn't make sense to generate Match extensions if there are no variants to match against.
    public bool SupportsExtensionMethods() => Namespace is not null && Variants.Count > 0;

    public bool SupportsImplicitConversions()
    {
        var allVariantsHaveSingleParameter = () =>
            Variants.All(static variant => variant.Parameters.Count is 1);

        var noVariantHasInterfaceParameter = () =>
            Variants
                .SelectMany(static variant => variant.Parameters)
                .All(static parameter => !parameter.Type.IsInterface);

        var allVariantsParameterTypesAreDifferent = () =>
        {
            var allParameterTypes = Variants
                .SelectMany(static variant => variant.Parameters)
                .Select(static parameter => parameter.Type.Identifier);
            var numAllParameterTypes = allParameterTypes.Count();
            var numUniqueParameterTypes = allParameterTypes.Distinct().Count();
            return numAllParameterTypes == numUniqueParameterTypes;
        };

        var unionHasNoRequiredProperties = () =>
            !Properties.Any(static property => property.IsRequired);

        return allVariantsHaveSingleParameter()
            && noVariantHasInterfaceParameter()
            && allVariantsParameterTypesAreDifferent()
            && unionHasNoRequiredProperties();
    }
}

internal sealed record VariantDeclaration
{
    public required string Identifier { get; init; }
    public required ImmutableEquatableArray<TypeParameter> TypeParameters { get; init; }
    public required ImmutableEquatableArray<Parameter> Parameters { get; init; }
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
