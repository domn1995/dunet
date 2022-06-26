namespace Dunet.UnionRecord;

internal record UnionRecord(
    List<string> Imports,
    string? Namespace,
    string Name,
    List<TypeParameter> TypeParameters,
    List<UnionRecordMember> Members
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
