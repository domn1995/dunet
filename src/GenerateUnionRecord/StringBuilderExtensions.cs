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

    /// <summary>
    /// Appends name of the given union, including each of its parent types separated by dots.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="union">The union to append the full name of.</param>
    public static void AppendFullUnionName(this StringBuilder builder, UnionRecord union)
    {
        var parentTypes = string.Join(".", union.ParentTypes);
        builder.Append(parentTypes);

        if (union.ParentTypes.Count > 0)
        {
            builder.Append(".");
        }

        builder.Append(union.Name);
    }
}
