﻿using Dunet.GenerateUnionRecord;
using System.Text;

namespace Dunet.GenerateUnionExtensions;

internal static class UnionExtensionsSource
{
    public static string GenerateExtensions(UnionRecord union)
    {
        if (union.Namespace is null)
        {
            throw new InvalidOperationException(
                "Cannot generate async match extensions if the union has no namespace."
            );
        }

        var builder = new StringBuilder();

        builder.AppendLine("#pragma warning disable 1591");

        foreach (var import in union.Imports)
        {
            builder.AppendLine(import);
        }

        builder.AppendLine();
        builder.AppendLine($"namespace {union.Namespace};");
        builder.AppendLine();

        builder.AppendLine(
            $"{union.Accessibility.ToKeyword()} static class {union.Name}MatchExtensions"
        );
        builder.AppendLine("{");
        var taskMethod = GenerateMatchAsyncMethod(union, "System.Threading.Tasks.Task");
        builder.AppendLine(taskMethod);
        var valueTaskMethod = GenerateMatchAsyncMethod(union, "System.Threading.Tasks.ValueTask");
        builder.Append(valueTaskMethod);
        builder.Append("}");

        return builder.ToString();
    }

    private static string GenerateMatchAsyncMethod(UnionRecord union, string taskType)
    {
        var builder = new StringBuilder();

        builder.Append($"    public static async {taskType}<TMatchOutput> MatchAsync");
        var methodTypeParams = union.TypeParameters.Prepend(new("TMatchOutput")).ToList();
        builder.AppendTypeParams(methodTypeParams);
        builder.AppendLine("(");
        builder.Append($"        this {taskType}<");
        builder.AppendFullUnionName(union);
        builder.AppendTypeParams(union.TypeParameters);
        builder.AppendLine("> unionTask,");

        for (int i = 0; i < union.Members.Count; ++i)
        {
            var member = union.Members[i];
            builder.Append($"        System.Func<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.Append($".{member.Name}");
            builder.Append($", TMatchOutput> {member.Name.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }

        builder.AppendLine($"    )");
        foreach (var typeParamConstraint in union.TypeParameterConstraints)
        {
            builder.AppendLine($"        {typeParamConstraint}");
        }
        builder.AppendLine($"    => (await unionTask.ConfigureAwait(false)).Match(");

        for (int i = 0; i < union.Members.Count; ++i)
        {
            var member = union.Members[i];
            builder.Append($"            {member.Name.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }

        builder.AppendLine("        );");

        builder.AppendLine("#pragma warning restore 1591");

        return builder.ToString();
    }
}
