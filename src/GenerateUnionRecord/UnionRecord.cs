using Microsoft.CodeAnalysis;

namespace Dunet.GenerateUnionRecord;

internal record UnionRecord(
    List<string> Imports,
    string? Namespace,
    Accessibility Accessibility,
    string Name,
    List<TypeParameter> TypeParameters,
    List<TypeParameterConstraint> TypeParameterConstraints,
    List<UnionRecordMember> Members,
    Stack<ParentType> ParentTypes
);

internal record UnionRecordMember(
    string Name,
    List<TypeParameter> TypeParameters,
    List<RecordProperty> Properties
);

internal record TypeParameter(string Name)
{
    public sealed override string ToString() => Name;
}

internal record RecordProperty(string Type, string Name);

/// <summary>
/// Represents a parent type declaration that nests a union record.
/// </summary>
/// <param name="IsRecord">Whether the type is a record or a plain class.</param>
/// <param name="Name">This type's name.</param>
internal sealed record ParentType(bool IsRecord, string Name)
{
    public sealed override string ToString() => Name;
}

/// <summary>
/// Represents a type parameter constraint on a union record's type parameters.
/// </summary>
/// <param name="Value">The full string value of the constraint. Ex: `where T : notnull, System.Exception`</param>
internal sealed record TypeParameterConstraint(string Value)
{
    public sealed override string ToString() => Value;
}
