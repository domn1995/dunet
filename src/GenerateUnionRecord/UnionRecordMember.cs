namespace Dunet.GenerateUnionRecord;

internal sealed class UnionRecordMember
{
    public required string Identifier { get; init; }
    public required List<TypeParameter> TypeParameters { get; init; }
    public required List<Property> Properties { get; init; }
}
