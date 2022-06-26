using System.Text;

namespace Dunet.GenerateUnionRecord;

internal static class StringBuilderExtensions
{
    public static void AppendTypeParams(
        this StringBuilder builder,
        IReadOnlyList<TypeParameter> typeParams
    )
    {
        if (typeParams.Count <= 0)
        {
            return;
        }

        var typeParamList = string.Join(", ", typeParams.Select(typeParam => typeParam.Name));

        builder.Append("<");
        builder.Append(typeParamList);
        builder.Append(">");
    }

    public static void AppendRecordProperties(
        this StringBuilder builder,
        IReadOnlyList<RecordProperty> properties
    )
    {
        var propertyList = string.Join(", ", properties.Select(p => p.ToString()));

        builder.Append("(");
        builder.Append(propertyList);
        builder.Append(")");
    }
}
