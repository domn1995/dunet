﻿using System.Reflection;
using System.Text;

namespace Dunet.Generator.UnionGeneration;

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

        var typeParamList = string.Join(
            ", ",
            typeParams.Select(static typeParam => typeParam.Identifier)
        );

        builder.Append("<");
        builder.Append(typeParamList);
        builder.Append(">");
    }

    /// <summary>
    /// Appends name of the given union, including each of its parent types separated by dots.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="union">The union to append the full name of.</param>
    public static void AppendFullUnionName(this StringBuilder builder, UnionDeclaration union)
    {
        var parentTypes = string.Join(".", union.ParentTypes);
        builder.Append(parentTypes);

        if (union.ParentTypes.Count > 0)
        {
            builder.Append(".");
        }

        builder.Append(union.Name);
    }

    public static StringBuilder AppendAutoGeneratedComment(this StringBuilder builder) =>
        builder.AppendLine("// <auto-generated/>");

    /// <summary>
    /// Appends the GeneratedCodeAttribute, to better enable the code to be detected as generated.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    public static StringBuilder AppendGeneratedCodeAttribute(this StringBuilder builder) =>
        builder.AppendLine(
            $$"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Dunet.Generator", "{{GeneratorVersion}}")]"""
        );

    private static string? GeneratorVersion { get; } = GetGeneratorVersion();

    private static string? GetGeneratorVersion()
    {
        var assembly = typeof(UnionGenerator).Assembly;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attribute?.InformationalVersion;
    }
}
