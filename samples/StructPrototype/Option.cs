using System.Diagnostics;

namespace StructPrototype;

public partial record struct Option<T>
{
    public record struct Some(T Value);

    public record struct None;
}

public partial record struct Option<T>
{
    private enum OptionType : byte
    {
        None,
        Some,
    };

    private OptionType type;
    private Some some;

    public TOut Match<TOut>(Func<Some, TOut> some, Func<TOut> none) =>
        type switch
        {
            OptionType.Some => some(this.some),
            OptionType.None => none(),
            var invalid
                => throw new UnreachableException($"Matched an unreachable union type: {invalid}"),
        };

    public static implicit operator Option<T>(T value) => Prelude.Some(value);

    public static class Prelude
    {
        public static Option<T> Some(T value) =>
            new() { type = OptionType.Some, some = new Some(value) };

        public static Option<T> None() => new() { type = OptionType.None };
    }
}

public static class OptionPrelude
{
    public static Option<T> Some<T>(T value) => Option<T>.Prelude.Some(value);

    public static Option<T> None<T>() => Option<T>.Prelude.None();
}
