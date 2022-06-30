namespace Dunet.UnionAttributeGeneration;

internal class UnionAttributeSource
{
    public const string AttributeNamespace = "Dunet";
    public const string AttributeName = "UnionAttribute";
    public const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    public static readonly string Attribute =
        $@"using System;

namespace {AttributeNamespace};

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed partial class {AttributeName} : Attribute
{{
}}";

    public static string GetPropertySource(UnionAttributeOptions defaultOptions) =>
        $@"using System;

namespace {AttributeNamespace};

public partial class {AttributeName}
{{
    /// Whether to generate static factory methods.
    /// Default: <c>{defaultOptions.GenerateFactoryMethods.ToString().ToLowerInvariant()}</c> 
    public bool GenerateFactoryMethods {{ get; set; }} = {defaultOptions.GenerateFactoryMethods.ToString().ToLowerInvariant()};

    /// Prefix for factory method names.
    /// Default: ""{defaultOptions.FactoryMethodPrefix}""
    public string FactoryMethodPrefix {{ get; set; }} = ""{defaultOptions.FactoryMethodPrefix}"";

    /// Suffix for factory method names.
    /// Default: ""{defaultOptions.FactoryMethodSuffix}""
    public string FactoryMethodSuffix {{ get; set; }} = ""{defaultOptions.FactoryMethodSuffix}"";
}}";
}
