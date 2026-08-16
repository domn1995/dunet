using System.Diagnostics;
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

    private readonly OptionType? type;
    private readonly Some? some;
    private readonly None? none;

    public Option(Some value)
    {
        this.type = OptionType.Some;
        this.some = value;
    }

    public Option(None value)
    {
        this.type = OptionType.None;
        this.none = value;
    }

    public bool HasValue => type is not null;

    public object? Value => type switch
    {
        OptionType.Some => some,
        OptionType.None => none,
        _ => null,
    };

    public bool TryGetValue(out Some? value)
    {
        value = this.some;
        return type is OptionType.Some;
    }

    public bool TryGetValue(out None? value)
    {
        value = this.none;
        return type is OptionType.None;
    }
}

public static class Option
{
    public static Option<T> Some<T>(T value) => new Option<T>.Some(value);

    public static Option<T> None<T>() => new Option<T>.None();
}
