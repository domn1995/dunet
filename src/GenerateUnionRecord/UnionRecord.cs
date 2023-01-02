using Microsoft.CodeAnalysis;

namespace Dunet.GenerateUnionRecord;

internal sealed record UnionRecord(
    List<string> Imports,
    string? Namespace,
    Accessibility Accessibility,
    string Name,
    List<TypeParameter> TypeParameters,
    List<TypeParameterConstraint> TypeParameterConstraints,
    List<UnionRecordMember> Members,
    Stack<ParentType> ParentTypes
)
{
    // Extension methods cannot be generated for a union declared in a top level program (no namespace).
    // It also doesn't make sense to generate Match extensions if there are no members to match aginst.
    public bool SupportsAsyncMatchExtensionMethods() => Namespace is not null && Members.Count > 0;
}

internal sealed record TypeParameter(string Identifier)
{
    public override string ToString() => Identifier;
}

internal sealed record Property(PropertyType Type, string Identifier);

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
