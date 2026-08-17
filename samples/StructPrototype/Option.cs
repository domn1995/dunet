using System.Runtime.CompilerServices;

namespace StructPrototype;

public partial record struct Option<T>
{
    public record struct Some(T Value);

    public record struct None;
}

[System.Runtime.CompilerServices.Union]
public readonly partial record struct Option<T> : IUnion
{
    private enum OptionType : byte
    {
        None,
        Some,
    };

    private static readonly None none;
    private readonly OptionType? type;
    private readonly T? some;

    public Option(T value)
    {
        this.type = OptionType.Some;
        this.some = value;
    }

    public Option(None _)
    {
        this.type = OptionType.None;
    }

    public bool HasValue => type is not null;

    public object? Value => type switch
    {
        OptionType.Some => some,
        OptionType.None => none,
        _ => null,
    };

    public bool TryGetValue(out T? value)
    {
        value = this.some;
        return type is OptionType.Some;
    }

    public bool TryGetValue(out None? value)
    {
        value = none;
        return type is OptionType.None;
    }
}

public static class Option
{
    public static Option<T> Some<T>(T value) => new(value);

    public static Option<T> None<T>() => new Option<T>.None();
}
