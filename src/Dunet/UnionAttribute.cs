namespace Dunet;

/// <summary>
/// Enables dunet union source generation for the decorated partial record.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute
{
    /// <summary>
    /// <para>
    /// Gets or initializes whether the union should support the direct assignment of each
    /// variant's underlying type directly to the union type.
    /// </para>
    /// <para>Defaults to <c>true</c>.</para>
    /// <para>
    /// If this is enabled, the generator will only create implicit conversions if each variant has
    /// exactly zero or one parameter and the parameter type is not an interface.
    /// </para>
    /// </summary>
    public bool EnableImplicitConversions { get; init; } = true;
}
