using Microsoft.CodeAnalysis;

namespace Dunet.Generator.UnionGeneration;

/// <summary>
/// Provides extension methods for an <see cref="IncrementalValuesProvider{TValues}"/>.
/// </summary>
internal static class IncrementalValuesProviderExtensions
{
    extension<T>(IncrementalValuesProvider<T?> self)
        where T : notnull
    {
        public IncrementalValuesProvider<T> Flatten() =>
            self.Where(static value => value is not null)!;
    }
}
