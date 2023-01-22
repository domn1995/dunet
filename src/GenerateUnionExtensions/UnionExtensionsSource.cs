using Dunet.GenerateUnionRecord;
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

        var taskMethodForFuncs = GenerateMatchAsyncMethodForFuncs(
            union,
            "System.Threading.Tasks.Task"
        );
        builder.AppendLine(taskMethodForFuncs);

        var valueTaskMethodForFuncs = GenerateMatchAsyncMethodForFuncs(
            union,
            "System.Threading.Tasks.ValueTask"
        );
        builder.AppendLine(valueTaskMethodForFuncs);

        var taskMethodForActions = GenerateMatchAsyncMethodForActions(
            union,
            "System.Threading.Tasks.Task"
        );
        builder.AppendLine(taskMethodForActions);

        var valueTaskMethodForActions = GenerateMatchAsyncMethodForActions(
            union,
            "System.Threading.Tasks.ValueTask"
        );
        builder.AppendLine(valueTaskMethodForActions);

        var specificTaskMethodsForFuncs = GenerateSpecificMatchAsyncMethodForFuncs(
            union,
            "System.Threading.Tasks.Task"
        );
        builder.AppendLine(specificTaskMethodsForFuncs);

        var specificValueTaskMethodsForFuncs = GenerateSpecificMatchAsyncMethodForFuncs(
            union,
            "System.Threading.Tasks.ValueTask"
        );
        builder.AppendLine(specificValueTaskMethodsForFuncs);

        builder.AppendLine("}");
        builder.AppendLine("#pragma warning restore 1591");

        return builder.ToString();
    }

    private static string GenerateMatchAsyncMethodForFuncs(UnionRecord union, string taskType)
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
            builder.Append($".{member.Identifier}");
            builder.Append($", TMatchOutput> {member.Identifier.ToMethodParameterCase()}");
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
            builder.Append($"            {member.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }

        builder.AppendLine("        );");

        return builder.ToString();
    }

    private static string GenerateMatchAsyncMethodForActions(UnionRecord union, string taskType)
    {
        var builder = new StringBuilder();

        builder.Append($"    public static async {taskType} MatchAsync");
        builder.AppendTypeParams(union.TypeParameters);
        builder.AppendLine("(");
        builder.Append($"        this {taskType}<");
        builder.AppendFullUnionName(union);
        builder.AppendTypeParams(union.TypeParameters);
        builder.AppendLine("> unionTask,");

        for (int i = 0; i < union.Members.Count; ++i)
        {
            var member = union.Members[i];
            builder.Append($"        System.Action<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.Append($".{member.Identifier}");
            builder.Append($"> {member.Identifier.ToMethodParameterCase()}");
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
            builder.Append($"            {member.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }

        builder.AppendLine("        );");

        return builder.ToString();
    }

    /// <summary>
    /// public static async Task<TMatchOuput> MatchSpecificAsync<T1, T2, ..., TMatchOutput>(
    ///     this Task<Parent1.Parent2.UnionType<T1, T2, ...>> unionTask,
    ///     System.Func<Parent1.Parent2.UnionType<T1, T2, ...>.Specific, TMatchOutput> @specific,
    ///     System.Func<TMatchOutput> @else
    /// )
    /// where T1 : notnull
    /// where T2 : notnull
    ///     =>
    ///         (await unionTask.ConfigureAwait(false))
    ///             .MatchSpecific(
    ///                 @specific,
    ///                 @else
    ///             );
    /// </summary>
    private static string GenerateSpecificMatchAsyncMethodForFuncs(
        UnionRecord union,
        string taskType
    )
    {
        var builder = new StringBuilder();

        foreach (var member in union.Members)
        {
            builder.Append(
                $"    public static async {taskType}<TMatchOutput> Match{member.Identifier}Async"
            );
            var methodTypeParams = union.TypeParameters.Append(new("TMatchOutput")).ToList();
            builder.AppendTypeParams(methodTypeParams);
            builder.AppendLine("(");
            builder.Append($"        this {taskType}<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.AppendLine("> unionTask,");
            builder.Append($"        System.Func<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.Append($".{member.Identifier}");
            builder.AppendLine($", TMatchOutput> {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine("        System.Func<TMatchOutput> @else");

            builder.AppendLine($"    )");
            foreach (var typeParamConstraint in union.TypeParameterConstraints)
            {
                builder.AppendLine($"    {typeParamConstraint}");
            }
            builder.AppendLine("        =>");
            builder.AppendLine($"            (await unionTask.ConfigureAwait(false))");
            builder.AppendLine($"                .Match{member.Identifier}(");
            builder.AppendLine($"                    {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine($"                    @else");
            builder.AppendLine("                );");
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
