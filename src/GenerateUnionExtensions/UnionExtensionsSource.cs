using Dunet.GenerateUnionRecord;
using System.Text;

namespace Dunet.GenerateUnionExtensions;

internal static class UnionExtensionsSource
{
    public static string GenerateExtensions(UnionRecord union)
    {
        var builder = new StringBuilder();

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Threading.Tasks;");

        foreach (var import in union.Imports)
        {
            builder.AppendLine(import);
        }

        if (union.Namespace is not null)
        {
            builder.AppendLine($"using {union.Namespace};");
        }

        builder.AppendLine();
        builder.AppendLine($"namespace Dunet.Extensions;");

        builder.AppendLine();
        builder.AppendLine(
            $"{union.Accessibility.ToKeyword()} static partial class {union.Name}MatchExtensions"
        );
        builder.AppendLine("{");
        var taskMethod = GenerateMatchAsyncMethod(union, "Task");
        builder.AppendLine(taskMethod);
        var valueTaskMethod = GenerateMatchAsyncMethod(union, "ValueTask");
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
            builder.Append($"        Func<");
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

        return builder.ToString();
    }
}
