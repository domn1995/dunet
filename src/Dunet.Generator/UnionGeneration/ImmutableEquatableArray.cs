using System.Collections;

namespace Dunet.Generator.UnionGeneration;

internal static class ImmutableEquatableArray
{
    public static ImmutableEquatableArray<T> ToImmutableEquatableArray<T>(
        this IEnumerable<T> values
    )
        where T : IEquatable<T> => new(values);
}

/// <summary>
/// Provides an immutable list implementation which implements sequence equality.
/// </summary>
internal sealed class ImmutableEquatableArray<T>(IEnumerable<T> values)
    : IEquatable<ImmutableEquatableArray<T>>,
        IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly T[] values = values.ToArray();

    public T this[int index] => values[index];

    public int Count => values.Length;

    public bool Equals(ImmutableEquatableArray<T>? other) =>
        other is not null && ((ReadOnlySpan<T>)values).SequenceEqual(other.values);

    public override bool Equals(object? obj) =>
        obj is ImmutableEquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (T value in values)
        {
            hash = Combine(hash, value.GetHashCode());
        }

        static int Combine(int h1, int h2)
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: https://github.com/dotnet/coreclr/pull/1830
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }

        return hash;
    }

    public Enumerator GetEnumerator() => new(values);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();

    public struct Enumerator
    {
        private readonly T[] values;
        private int index;

        internal Enumerator(T[] values)
        {
            this.values = values;
            index = -1;
        }

        public bool MoveNext()
        {
            var newIndex = index + 1;

            if ((uint)newIndex < (uint)values.Length)
            {
                index = newIndex;
                return true;
            }

            return false;
        }

        public readonly T Current => values[index];
    }
}
