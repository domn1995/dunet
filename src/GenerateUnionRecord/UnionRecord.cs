namespace Dunet.GenerateUnionRecord;

internal record UnionRecord(
    List<string> Imports,
    string? Namespace,
    string Name,
    List<TypeParameter> TypeParameters,
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
internal record ParentType(bool IsRecord, string Name);
