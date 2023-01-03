using System.Diagnostics;

namespace StructPrototype;

public record struct Option<T> where T : notnull
{
    public record struct Some(T Value);

    public record struct None;

    private enum OptionType : byte
    {
        None,
        Some,
    };

    private OptionType type;
    private T some;

    public TOut Match<TOut>(Func<T, TOut> some, Func<TOut> none) =>
        type switch
        {
            OptionType.Some => some(this.some),
            OptionType.None => none(),
            var invalid
                => throw new UnreachableException($"Matched an unreachable union type: {invalid}"),
        };

    public static Option<T> NewSome(T value) => new() { type = OptionType.Some, some = value };

    public static Option<T> NewNone() => new() { type = OptionType.None };

    public static implicit operator Option<T>(T value) => NewSome(value);

    public T UnwrapSome() =>
        type switch
        {
            OptionType.Some => some,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {OptionType.Some} but got {actual}"
                ),
        };
}

public static class OptionPrelude
{
    public static Option<T> Some<T>(T value) where T : notnull => Option<T>.NewSome(value);

    public static Option<T> None<T>() where T : notnull => Option<T>.NewNone();
}
