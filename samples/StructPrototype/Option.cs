using System.Diagnostics;

namespace StructPrototype;

public partial record struct Option<T>
{
    public readonly record struct Some(T Value);

    public readonly record struct None;
}

public readonly partial record struct Option<T>
{
    private enum OptionType : byte
    {
        None,
        Some,
    };

    private Option(Some @some)
    {
        this.@some = @some;
        type = OptionType.Some;
    }

    private Option(None @none)
    {
        this.@none = @none;
        type = OptionType.None;
    }

    private readonly OptionType type;
    private readonly Some @some;
    private readonly None @none;

    public TOut Match<TOut>(Func<Some, TOut> @some, Func<None, TOut> @none) =>
        type switch
        {
            OptionType.Some => @some(this.@some),
            OptionType.None => @none(this.@none),
            var invalid
                => throw new UnreachableException($"Matched an unreachable union type: {invalid}"),
        };

    public static implicit operator Option<T>(T value) => NewSome(value);

    public static Option<T> NewSome(T @value) => new(new Some(@value));

    public static Option<T> NewNone() => new(new None());
}
