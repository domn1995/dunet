namespace Dunet.UnionAttributeGeneration;

internal class UnionAttributeSource
{
    public const string AttributeNamespace = "Dunet";
    public const string AttributeName = "UnionAttribute";
    public const string FullAttributeName = $"{AttributeNamespace}.{AttributeName}";

    public const string Attribute =
        @"using System;

namespace Dunet;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute {}";
}
