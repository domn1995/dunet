using System.Text;

namespace Dunet;

internal static class UnionSource
{
    public const string AttributeNamespace = "Dunet";
    public const string AttributeName = "UnionAttribute";
    public const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    public const string Attribute =
        @"
namespace Dunet;

[System.AttributeUsage(System.AttributeTargets.Interface)]
public class UnionAttribute : System.Attribute
{
}";

    public static string GenerateRecord(RecordToGenerate recordToGenerate)
    {
        var propertiesCount = recordToGenerate.Properties.Count;
        var interfaceMethodsCount = recordToGenerate.Methods.Count;
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

        for (int i = 0; i < propertiesCount; ++i)
        {
            var (type, name) = recordToGenerate.Properties[i];
            builder.Append($"{type} {name}{(i != propertiesCount - 1 ? "," : "")}");
        }

        builder.AppendLine($") : {interfaceName}");
        builder.AppendLine("{");

        for (int i = 0; i < interfaceMethodsCount; ++i)
        {
            var interfaceMethod = recordToGenerate.Methods[i];
            var parametersCount = recordToGenerate.Methods[i].Parameters.Count;
            var methodReturnType = interfaceMethod.ReturnType;
            var methodName = interfaceMethod.Name;
            builder.Append($"    {methodReturnType} {interfaceName}.{methodName}(");

            for (int j = 0; j < parametersCount; ++j)
            {
                var parameterType = interfaceMethod.Parameters[j].Type;
                var parameterName = interfaceMethod.Parameters[j].Name;
                builder.Append(
                    $"{parameterType} {parameterName}{(j != parametersCount - 1 ? ", " : "")}"
                );
            }

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

        var parameters = methodToGenerate.Parameters;
        for (int i = 0; i < parameters.Count; ++i)
        {
            var parameter = parameters[i];
            builder.AppendLine(
                $"        Func<{parameter.Type}, TResult> {parameter.Name}{(i != parameters.Count - 1 ? ", " : "")}"
            );
        }

        builder.AppendLine("    )");
        builder.Append("    {");

        for (int i = 0; i < parameters.Count; ++i)
        {
            var parameter = parameters[i];
            builder.Append(
                @$"
        if (type is {parameter.Type} t{i})
        {{
            return {parameter.Name}(t{i});
        }}
"
            );
        }

        builder.AppendLine("        throw new InvalidOperationException();");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }
}
