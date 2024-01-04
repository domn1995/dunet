using Microsoft.CodeAnalysis;

namespace Dunet.Generator.UnionGeneration;

/// <summary>
/// Provides extension methods for an <see cref="IncrementalValuesProvider{TValues}"/>.
/// </summary>
internal static class IncrementalValuesProviderExtensions
{
    /// <summary>
    /// Removes <see langword="null"/> values from this provider.
    /// </summary>
    /// <typeparam name="T">The type of this provider's values.</typeparam>
    /// <param name="provider">This incremental values provider.</param>
    /// <returns>A new incremental values provider without <see langword="null"/> values.</returns>
    public static IncrementalValuesProvider<T> Flatten<T>(
        this IncrementalValuesProvider<T?> provider
    )
        where T : notnull => provider.Where(static value => value is not null)!;
}
