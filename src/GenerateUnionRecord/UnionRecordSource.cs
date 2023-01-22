﻿using System.Text;

namespace Dunet.GenerateUnionRecord;

internal static class UnionRecordSource
{
    public static string GenerateRecord(UnionRecord record)
    {
        var builder = new StringBuilder();

        builder.AppendLine("#pragma warning disable 1591");

        foreach (var import in record.Imports)
        {
            builder.AppendLine(import);
        }

        if (record.Namespace is not null)
        {
            builder.AppendLine($"namespace {record.Namespace};");
        }

        var parentTypes = record.ParentTypes;

        foreach (var type in parentTypes)
        {
            builder.AppendLine($"partial {(type.IsRecord ? "record" : "class")} {type.Identifier}");
            builder.AppendLine("{");
        }

        builder.Append($"abstract partial record {record.Name}");
        builder.AppendTypeParams(record.TypeParameters);
        builder.AppendLine();
        builder.AppendLine("{");
        builder.AppendLine($"    private {record.Name}() {{}}");
        builder.AppendLine();

        // Func match method.
        builder.AppendLine("    public abstract TMatchOutput Match<TMatchOutput>(");
        for (int i = 0; i < record.Members.Count; ++i)
        {
            var member = record.Members[i];
            builder.Append($"        System.Func<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($", TMatchOutput> {member.Identifier.ToMethodParameterCase()}");
            if (i < record.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("    );");

        // Action match method.
        builder.AppendLine("    public abstract void Match(");
        for (int i = 0; i < record.Members.Count; ++i)
        {
            var member = record.Members[i];
            builder.Append($"        System.Action<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($"> {member.Identifier.ToMethodParameterCase()}");
            if (i < record.Members.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("    );");

        builder.AppendLine();

        // Specific func match methods.
        foreach (var member in record.Members)
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

        // Specific action match methods.
        foreach (var member in record.Members)
        {
            builder.AppendLine($"    public abstract void Match{member.Identifier}(");
            builder.Append($"        System.Action<{member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.AppendLine($"> {member.Identifier.ToMethodParameterCase()},");
            builder.AppendLine($"        System.Action @else");
            builder.AppendLine("    );");
        }

        if (SupportsImplicitConversions(record))
        {
            foreach (var member in record.Members)
            {
                builder.Append($"    public static implicit operator {record.Name}");
                builder.AppendTypeParams(record.TypeParameters);
                builder.AppendLine(
                    $"({member.Properties[0].Type.Identifier} value) => new {member.Identifier}(value);"
                );
            }
            builder.AppendLine();
        }

        foreach (var member in record.Members)
        {
            builder.Append($"    public sealed partial record {member.Identifier}");
            builder.AppendTypeParams(member.TypeParameters);
            builder.Append($" : {record.Name}");
            builder.AppendTypeParams(record.TypeParameters);
            builder.AppendLine();
            builder.AppendLine("    {");

            // Func match method.
            builder.AppendLine("        public override TMatchOutput Match<TMatchOutput>(");
            for (int i = 0; i < record.Members.Count; ++i)
            {
                var memberParam = record.Members[i];
                builder.Append($"            System.Func<{memberParam.Identifier}");
                builder.AppendTypeParams(memberParam.TypeParameters);
                builder.Append($", TMatchOutput> {memberParam.Identifier.ToMethodParameterCase()}");
                if (i < record.Members.Count - 1)
                {
                    builder.Append(",");
                }
                builder.AppendLine();
            }
            builder.AppendLine($"        ) => {member.Identifier.ToMethodParameterCase()}(this);");

            // Action match method.
            builder.AppendLine("        public override void Match(");
            for (int i = 0; i < record.Members.Count; ++i)
            {
                var memberParam = record.Members[i];
                builder.Append($"            System.Action<{memberParam.Identifier}");
                builder.AppendTypeParams(memberParam.TypeParameters);
                builder.Append($"> {memberParam.Identifier.ToMethodParameterCase()}");
                if (i < record.Members.Count - 1)
                {
                    builder.Append(",");
                }
                builder.AppendLine();
            }
            builder.AppendLine($"        ) => {member.Identifier.ToMethodParameterCase()}(this);");

            // Specific func match methods.
            foreach (var specificMember in record.Members)
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
                    builder.AppendLine(
                        $"{specificMember.Identifier.ToMethodParameterCase()}(this);"
                    );
                }
                else
                {
                    builder.AppendLine("@else();");
                }
            }

            // Specific action match methods.
            foreach (var specificMember in record.Members)
            {
                builder.AppendLine(
                    $"        public override void Match{specificMember.Identifier}("
                );
                builder.Append($"            System.Action<{specificMember.Identifier}");
                builder.AppendTypeParams(specificMember.TypeParameters);
                builder.AppendLine($"> {specificMember.Identifier.ToMethodParameterCase()},");
                builder.AppendLine($"            System.Action @else");
                builder.Append("        ) => ");
                if (specificMember.Identifier == member.Identifier)
                {
                    builder.AppendLine(
                        $"{specificMember.Identifier.ToMethodParameterCase()}(this);"
                    );
                }
                else
                {
                    builder.AppendLine("@else();");
                }
            }

            builder.AppendLine("    }");
            builder.AppendLine();
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
}
