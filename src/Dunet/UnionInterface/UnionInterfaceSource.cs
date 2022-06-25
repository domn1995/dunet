using System.Text;

namespace Dunet.UnionInterface;

internal static class UnionInterfaceSource
{
    public static string GenerateRecord(RecordToGenerate recordToGenerate)
    {
        var properties = recordToGenerate.Properties.Select(static prop => prop.ToString());
        var interfaceName = recordToGenerate.Interface;
        var builder = new StringBuilder();

        foreach (var import in recordToGenerate.Imports)
        {
            builder.AppendLine(import);
        }

        if (recordToGenerate.Namespace is not null)
        {
            builder.AppendLine($"namespace {recordToGenerate.Namespace};");
            builder.AppendLine();
        }

        builder.Append($"public record {recordToGenerate.Name}(");
        builder.Append(string.Join(", ", properties));
        builder.AppendLine($") : {interfaceName}");
        builder.AppendLine("{");

        foreach (var interfaceMethod in recordToGenerate.Methods)
        {
            var parameters = interfaceMethod.Parameters.Select(static param => param.ToString());
            var methodReturnType = interfaceMethod.ReturnType;
            var methodName = interfaceMethod.Name;
            builder.Append($"    {methodReturnType} {interfaceName}.{methodName}(");
            builder.Append(string.Join(", ", parameters));
            builder.AppendLine(") => throw new System.InvalidOperationException();");
        }

        builder.Append("}");

        return builder.ToString();
    }

    public static string GenerateMatchMethod(MatchMethodToGenerate methodToGenerate)
    {
        var builder = new StringBuilder();

        builder.AppendLine("using System;");

        foreach (var import in methodToGenerate.Imports)
        {
            builder.AppendLine(import);
        }

        if (methodToGenerate.Namespace is not null)
        {
            builder.AppendLine($"namespace {methodToGenerate.Namespace};");
        }

        var accessibility = methodToGenerate.Accessibility.ToKeyword();
        builder.AppendLine(
            $"{accessibility} static class {methodToGenerate.Interface}DiscriminatedUnionExtensions"
        );
        builder.AppendLine("{");

        builder.AppendLine("    public static TResult Match<TResult>(");
        builder.AppendLine($"        this {methodToGenerate.Interface} type, ");

        var parameters = methodToGenerate.Parameters.Select(
            param => $"        Func<{param.Type}, TResult> {param.Name}"
        );

        builder.AppendLine(string.Join(",\n", parameters));

        builder.AppendLine("    )");
        builder.Append("    {");

        var typeChecks = methodToGenerate.Parameters.Select(
            param =>
                @$"
        if (type is {param.Type} t{param.Type})
        {{
            return {param.Name}(t{param.Type});
        }}
"
        );

        builder.AppendLine(string.Join("", typeChecks));

        builder.AppendLine("        throw new InvalidOperationException();");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }
}
