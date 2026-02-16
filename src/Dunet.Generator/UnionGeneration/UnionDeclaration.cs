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
    ImmutableEquatableArray<Property> Properties,
    bool IsImplicitConversionsEnabled = true
)
{
    // Extension methods cannot be generated for a union declared in a top level program (no namespace).
    // It also doesn't make sense to generate Match extensions if there are no variants to match against.
    public bool SupportsExtensionMethods() => Namespace is not null && Variants.Count > 0;

    public List<VariantDeclaration> VariantsWithImplicitConversionSupport()
    {
        if (!IsImplicitConversionsEnabled)
        {
            return [];
        }

        var hasRequiredProperties = Properties.Any(static property => property.IsRequired);

        // We cannot generate implicit conversions for unions with required properties because
        // we cannot initialize the required value as part of performing the conversion.
        if (hasRequiredProperties)
        {
            return [];
        }

        static bool eachVariantHasUniqueParameterType(IEnumerable<VariantDeclaration> variants)
        {
            var allParameterTypes = variants
                // Ignore variants with no parameters since they don't impact uniqueness.
                .Where(static variant => variant.Parameters.Count > 0)
                .SelectMany(static variant => variant.Parameters)
                // Strip nullable annotation (?) to compare base types for uniqueness
                // since C# doesn't allow separate implicit conversions for `T` and `T?`.
                .Select(static parameter => parameter.Type.Identifier.TrimEnd('?'))
                .ToList();
            var numAllParameterTypes = allParameterTypes.Count;
            var numUniqueParameterTypes = allParameterTypes.Distinct().Count();
            return numAllParameterTypes == numUniqueParameterTypes;
        }

        if (!eachVariantHasUniqueParameterType(Variants))
        {
            return [];
        }

        // Isolate the variants that have a single parameter because only those can have implicit conversions.
        bool hasSingleParameter(VariantDeclaration variant) => variant.Parameters.Count is 1;

        static bool hasInterfaceParameter(VariantDeclaration variant) =>
            variant.Parameters.Any(static parameter => parameter.Type.IsInterface);

        return Variants
            .Where(variant => hasSingleParameter(variant) && !hasInterfaceParameter(variant))
            .ToList();
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
