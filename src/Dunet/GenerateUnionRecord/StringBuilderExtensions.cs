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
}
