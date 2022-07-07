namespace Dunet.UnionAttributeGeneration;

internal class UnionAttributeSource
{
    public const string Namespace = "Dunet";
    public const string Name = "UnionAttribute";
    public const string FullyQualifiedName = $"{Namespace}.{Name}";

    public const string SourceCode =
        @"using System;

namespace Dunet;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute {}";
}
