namespace Dunet.UnionAttributeGeneration;

internal class UnionAttributeSource
{
    public const string AttributeNamespace = "Dunet";
    public const string AttributeName = "UnionAttribute";
    public const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    public static readonly string Attribute =
        $@"using System;

namespace Dunet;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute
{{
    public bool GenerateFactoryMethods {{ get; set; }} = {UnionAttributeOptions.Default.GenerateFactoryMethods.ToString().ToLowerInvariant()};
    public string FactoryMethodPrefix {{ get; set; }} = ""{UnionAttributeOptions.Default.FactoryMethodPrefix}"";
    public string FactoryMethodSuffix {{ get; set; }} = ""{UnionAttributeOptions.Default.FactoryMethodSuffix}"";
}}";
}
