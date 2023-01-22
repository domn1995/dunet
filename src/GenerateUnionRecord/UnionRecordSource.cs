using System.Text;

namespace Dunet.GenerateUnionRecord;

internal static class UnionRecordSource
{
    public static string GenerateRecord(UnionRecord union)
    {
        var builder = new StringBuilder();

        builder.AppendLine("#pragma warning disable 1591");

        foreach (var import in union.Imports)
        {
            builder.AppendLine(import);
        }

        if (union.Namespace is not null)
        {
            builder.AppendLine($"namespace {union.Namespace};");
        }

        var parentTypes = union.ParentTypes;

        foreach (var type in parentTypes)
        {
            builder.AppendLine($"partial {(type.IsRecord ? "record" : "class")} {type.Identifier}");
            builder.AppendLine("{");
        }

        builder.Append($"abstract partial record {union.Name}");
        builder.AppendTypeParams(union.TypeParameters);
        builder.AppendLine();
        builder.AppendLine("{");
        builder.AppendLine($"    private {union.Name}() {{}}");
        builder.AppendLine();

        var abstractMatchMethods = GenerateAbstractMatchMethods(union);
        builder.AppendLine(abstractMatchMethods);
        builder.AppendLine();

        var abstractSpecificMatchMethods = GenerateAbstractSpecificMatchMethods(union);
        builder.AppendLine(abstractSpecificMatchMethods);
        builder.AppendLine();

        if (SupportsImplicitConversions(union))
        {
            foreach (var member in union.Members)
            {
                builder.Append($"    public static implicit operator {union.Name}");
                builder.AppendTypeParams(union.TypeParameters);
                builder.AppendLine(
                    $"({member.Properties[0].Type.Identifier} value) => new {member.Identifier}(value);"
                );
            }
            builder.AppendLine();
        }

        foreach (var member in union.Members)
        {
            builder.Append($"    public sealed partial record {member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($" : {union.Name}");
            builder.AppendTypeParams(union.TypeParameters);
            builder.AppendLine();
            builder.AppendLine("    {");

            var matchMethodImplementations = GenerateMatchMethodImplementationsForMember(
                union,
                member
            );
            builder.AppendLine(matchMethodImplementations);

            var specificMatchMethodImplementations =
                GenerateSpecificMatchMethodImplementationsForMember(union, member);
            builder.AppendLine(specificMatchMethodImplementations);
        }

        builder.AppendLine("}");

        foreach (var _ in parentTypes)
        {
            builder.AppendLine("}");
        }

        builder.AppendLine("#pragma warning restore 1591");

        return builder.ToString();
    }

    private static bool SupportsImplicitConversions(UnionRecord union)
    {
        var membersHaveSingleParameter = () =>
            union.Members.All(member => member.Properties.Count is 1);

        var membersHaveNoInterfaceParameters = () =>
            !union.Members
                .SelectMany(member => member.Properties)
                .Any(prop => prop.Type.IsInterface);

        var membersHaveUniqueParameterTypes = () =>
        {
            var allPropertyTypes = union.Members
                .SelectMany(member => member.Properties)
                .Select(prop => prop.Type);
            var allPropertyTypesCount = allPropertyTypes.Count();
            var uniquePropertyTypesCount = allPropertyTypes.Distinct().Count();
            return allPropertyTypesCount == uniquePropertyTypesCount;
        };

        return membersHaveSingleParameter()
            && membersHaveNoInterfaceParameters()
            && membersHaveUniqueParameterTypes();
    }

    private static string GenerateAbstractMatchMethods(UnionRecord union)
    {
        var builder = new StringBuilder();

        builder.AppendLine("    public abstract TMatchOutput Match<TMatchOutput>(");
        for (int i = 0; i < union.Members.Count; ++i)
        {
            var member = union.Members[i];
            builder.Append($"        System.Func<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($", TMatchOutput> {member.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("    );");

        // Action match method.
        builder.AppendLine("    public abstract void Match(");
        for (int i = 0; i < union.Members.Count; ++i)
        {
            var member = union.Members[i];
            builder.Append($"        System.Action<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($"> {member.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("    );");

        return builder.ToString();
    }

    private static string GenerateAbstractSpecificMatchMethods(UnionRecord union)
    {
        var builder = new StringBuilder();

        foreach (var member in union.Members)
        {
            builder.AppendLine(
                $"    public abstract TMatchOutput Match{member.Identifier}<TMatchOutput>("
            );
            builder.Append($"        System.Func<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.AppendLine($", TMatchOutput> {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine($"        System.Func<TMatchOutput> @else");
            builder.AppendLine("    );");
        }

        builder.AppendLine();

        foreach (var member in union.Members)
        {
            builder.AppendLine($"    public abstract void Match{member.Identifier}(");
            builder.Append($"        System.Action<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.AppendLine($"> {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine($"        System.Action @else");
            builder.AppendLine("    );");
        }

        return builder.ToString();
    }

    private static string GenerateMatchMethodImplementationsForMember(
        UnionRecord union,
        UnionRecordMember member
    )
    {
        var builder = new StringBuilder();

        builder.AppendLine("        public override TMatchOutput Match<TMatchOutput>(");
        for (int i = 0; i < union.Members.Count; ++i)
        {
            var memberParam = union.Members[i];
            builder.Append($"            System.Func<{memberParam.Identifier}");
            builder.AppendTypeParams(memberParam.TypeParameters);
            builder.Append($", TMatchOutput> {memberParam.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine($"        ) => {member.Identifier.ToMethodParameterCase()}(this);");

        // Action match method.
        builder.AppendLine("        public override void Match(");
        for (int i = 0; i < union.Members.Count; ++i)
        {
            var memberParam = union.Members[i];
            builder.Append($"            System.Action<{memberParam.Identifier}");
            builder.AppendTypeParams(memberParam.TypeParameters);
            builder.Append($"> {memberParam.Identifier.ToMethodParameterCase()}");
            if (i < union.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine($"        ) => {member.Identifier.ToMethodParameterCase()}(this);");

        return builder.ToString();
    }

    private static string GenerateSpecificMatchMethodImplementationsForMember(
        UnionRecord union,
        UnionRecordMember member
    )
    {
        var builder = new StringBuilder();

        // Specific func match methods.
        foreach (var specificMember in union.Members)
        {
            builder.AppendLine(
                $"        public override TMatchOutput Match{specificMember.Identifier}<TMatchOutput>("
            );
            builder.Append($"            System.Func<{specificMember.Identifier}");
            builder.AppendTypeParams(specificMember.TypeParameters);
            builder.AppendLine(
                $", TMatchOutput> {specificMember.Identifier.ToMethodParameterCase()},"
            );
            builder.AppendLine($"            System.Func<TMatchOutput> @else");
            builder.Append("        ) => ");
            if (specificMember.Identifier == member.Identifier)
            {
                builder.AppendLine($"{specificMember.Identifier.ToMethodParameterCase()}(this);");
            }
            else
            {
                builder.AppendLine("@else();");
            }
        }

        // Specific action match methods.
        foreach (var specificMember in union.Members)
        {
            builder.AppendLine($"        public override void Match{specificMember.Identifier}(");
            builder.Append($"            System.Action<{specificMember.Identifier}");
            builder.AppendTypeParams(specificMember.TypeParameters);
            builder.AppendLine($"> {specificMember.Identifier.ToMethodParameterCase()},");
            builder.AppendLine($"            System.Action @else");
            builder.Append("        ) => ");
            if (specificMember.Identifier == member.Identifier)
            {
                builder.AppendLine($"{specificMember.Identifier.ToMethodParameterCase()}(this);");
            }
            else
            {
                builder.AppendLine("@else();");
            }
        }

        builder.AppendLine("    }");
        builder.AppendLine();

        return builder.ToString();
    }
}
