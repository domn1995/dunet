using Dunet.GenerateUnionRecord;
using System.Text;

namespace Dunet.GenerateUnionExtensions;

internal static class UnionExtensionsSourceBuilder
{
    const string task = "System.Threading.Tasks.Task";
    const string valueTask = "System.Threading.Tasks.ValueTask";

    public static string GenerateExtensions(UnionRecord union)
    {
        if (union.Namespace is null)
        {
            throw new InvalidOperationException(
                "Cannot generate async match extensions if the union has no namespace."
            );
        }

        return new StringBuilder()
            .AppendLine("#pragma warning disable 1591")
            .AppendUsingStatements(union)
            .AppendLine()
            .AppendLine($"namespace {union.Namespace};")
            .AppendLine()
            .AppendExtensionClassDeclaration(union)
            .AppendLine("{")
            .AppendMatchAsyncMethodForFuncs(union, task)
            .AppendMatchAsyncMethodForFuncs(union, valueTask)
            .AppendMatchAsyncMethodForActions(union, task)
            .AppendMatchAsyncMethodForActions(union, valueTask)
            .AppendSpecificMatchAsyncMethodForFuncs(union, task)
            .AppendSpecificMatchAsyncMethodForFuncs(union, valueTask)
            .AppendSpecificMatchAsyncMethodForActions(union, task)
            .AppendSpecificMatchAsyncMethodForActions(union, valueTask)
            .AppendLine("}")
            .AppendLine("#pragma warning restore 1591")
            .ToString();
    }

    private static StringBuilder AppendExtensionClassDeclaration(
        this StringBuilder builder,
        UnionRecord union
    ) =>
        builder.AppendLine(
            $"{union.Accessibility.ToKeyword()} static class {union.Name}MatchExtensions"
        );

    private static StringBuilder AppendUsingStatements(
        this StringBuilder builder,
        UnionRecord union
    )
    {
        foreach (var import in union.Imports)
        {
            builder.AppendLine(import);
        }

        return builder;
    }

    private static StringBuilder AppendMatchAsyncMethodForFuncs(
        this StringBuilder builder,
        UnionRecord union,
        string taskType
    )
    {
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

        return builder;
    }

    private static StringBuilder AppendMatchAsyncMethodForActions(
        this StringBuilder builder,
        UnionRecord union,
        string taskType
    )
    {
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

        return builder;
    }

    /// <summary>
    /// public static async TaskType<TMatchOuput> MatchSpecificAsync<T1, T2, ..., TMatchOutput>(
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
    private static StringBuilder AppendSpecificMatchAsyncMethodForFuncs(
        this StringBuilder builder,
        UnionRecord union,
        string taskType
    )
    {
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
        }

        return builder;
    }

    /// <summary>
    /// public static async TaskType MatchSpecificAsync<T1, T2, ...>(
    ///     this Task<Parent1.Parent2.UnionType<T1, T2, ...>> unionTask,
    ///     System.Action<Parent1.Parent2.UnionType<T1, T2, ...>.Specific> @specific,
    ///     System.Action @else
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
    private static StringBuilder AppendSpecificMatchAsyncMethodForActions(
        this StringBuilder builder,
        UnionRecord union,
        string taskType
    )
    {
        foreach (var member in union.Members)
        {
            builder.Append($"    public static async {taskType} Match{member.Identifier}Async");
            builder.AppendTypeParams(union.TypeParameters);
            builder.AppendLine("(");
            builder.Append($"        this {taskType}<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.AppendLine("> unionTask,");
            builder.Append($"        System.Action<");
            builder.AppendFullUnionName(union);
            builder.AppendTypeParams(union.TypeParameters);
            builder.Append($".{member.Identifier}");
            builder.AppendLine($"> {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine("        System.Action @else");

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
        }

        return builder;
    }
}
