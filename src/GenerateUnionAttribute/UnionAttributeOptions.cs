using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet.UnionAttributeGeneration;

record UnionAttributeOptions(bool GenerateFactoryMethods, string FactoryMethodPrefix, string FactoryMethodSuffix)
{
    public static readonly UnionAttributeOptions Default = new(true, "New", "");

    public UnionAttributeOptions WithAttributeArgument(GeneratorSyntaxContext context,
        AttributeArgumentSyntax argument)
    {
        if (context.SemanticModel.GetConstantValue(argument.Expression) is not { HasValue: true, Value: {} propertyValue })
            return this;

        return argument.NameEquals?.Name.Identifier.Text switch
        {
            nameof(GenerateFactoryMethods) => this with
            {
                GenerateFactoryMethods = (bool) propertyValue
            },
            nameof(FactoryMethodPrefix) => this with
            {
                FactoryMethodPrefix = (string) propertyValue
            },
            nameof(FactoryMethodSuffix) => this with
            {
                FactoryMethodSuffix = (string) propertyValue
            },
            null => this,
            // Unreachable
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
