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

        builder.Append(
            @$"
namespace {recordToGenerate.Namespace};

public record {recordToGenerate.Name}("
        );

        for (int i = 0; i < propertiesCount; ++i)
        {
            var (type, name) = recordToGenerate.Properties[i];
            builder.Append(
                @$"
    {type} {name}{(i != propertiesCount - 1 ? "," : "")}"
            );
        }

        builder.Append(
            $@"
) : {interfaceName}
{{"
        );

        for (int i = 0; i < interfaceMethodsCount; ++i)
        {
            var interfaceMethod = recordToGenerate.Methods[i];
            var parametersCount = recordToGenerate.Methods[i].Parameters.Count;
            var methodName = interfaceMethod.Name;
            builder.Append(
                $@"
    {interfaceName} {interfaceName}.{methodName}("
            );
            for (int j = 0; j < parametersCount; ++j)
            {
                var parameterType = interfaceMethod.Parameters[j].Type;
                var parameterName = interfaceMethod.Parameters[j].Name;
                builder.Append(
                    $"{parameterType} {parameterName}{(j != parametersCount - 1 ? ", " : ")")}"
                );
            }

            builder.AppendLine(" => throw new System.InvalidOperationException();");
        }

        builder.Append(
            @"
}"
        );

        return builder.ToString();
    }
}
