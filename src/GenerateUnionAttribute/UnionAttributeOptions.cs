using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dunet.UnionAttributeGeneration;

record UnionAttributeOptions(bool GenerateFactoryMethods, string FactoryMethodPrefix, string FactoryMethodSuffix)
{
    public static UnionAttributeOptions CreateDefault(AnalyzerConfigOptionsProvider configOptions)
    {
        if (!configOptions.GlobalOptions.TryGetValue("build_property.Dunet_GenerateFactoryMethods",
                out var generateFactoryMethodsStr)
            || !bool.TryParse(generateFactoryMethodsStr, out var generateFactoryMethods))
        {
            throw new Exception($"Dunet_GenerateFactoryMethods MSBuild property must be either true or false.  Actual: {generateFactoryMethodsStr}");
        }

        if (!configOptions.GlobalOptions.TryGetValue("build_property.Dunet_FactoryMethodPrefix",
                out var factoryMethodPrefix))
        {
            throw new Exception("Dunet_FactoryMethodPrefix MSBuild property missing");
        }
        factoryMethodPrefix = factoryMethodPrefix.Trim();

        if (!configOptions.GlobalOptions.TryGetValue("build_property.Dunet_FactoryMethodSuffix",
                out var factoryMethodSuffix))
        {
            throw new Exception("Dunet_FactoryMethodSuffix MSBuild property missing");
        }
        factoryMethodSuffix = factoryMethodSuffix.Trim();

        return new UnionAttributeOptions(generateFactoryMethods, factoryMethodPrefix, factoryMethodSuffix);
    }

    public UnionAttributeOptions WithAttributeArgument(SemanticModel semanticModel,
        AttributeArgumentSyntax argument)
    {
        if (semanticModel.GetConstantValue(argument.Expression) is not { HasValue: true, Value: { } propertyValue })
            return this;

        return argument.NameEquals?.Name.Identifier.Text switch
        {
            nameof(GenerateFactoryMethods) => this with
            {
                GenerateFactoryMethods = (bool)propertyValue
            },
            nameof(FactoryMethodPrefix) => this with
            {
                FactoryMethodPrefix = (string)propertyValue
            },
            nameof(FactoryMethodSuffix) => this with
            {
                FactoryMethodSuffix = (string)propertyValue
            },
            null => this,
            // Unreachable
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
