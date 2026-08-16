using System.Runtime.CompilerServices;

namespace StructPrototype;

public partial record struct Result<TErr, TOk>
{
    public record struct Ok(TOk Value);

    public record struct Err(TErr Error);
}

[System.Runtime.CompilerServices.Union]
public readonly partial record struct Result<TErr, TOk> : IUnion
{
    private enum ResultType : byte
    {
        Err,
        Ok,
    };

    private readonly ResultType? type;

    private readonly TOk? ok;
    private readonly TErr? err;

    public bool HasValue => type is not null;

    public object? Value => type switch
    {
        ResultType.Ok => ok,
        ResultType.Err => err,
        _ => null,
    };

    public Result(TOk ok)
    {
        this.type = ResultType.Ok;
        this.ok = ok;
    }

    public Result(TErr err)
    {
        this.type = ResultType.Err;
        this.err = err;
    }

    public bool TryGetValue(out TOk? value)
    {
        value = this.ok;
        return type is ResultType.Ok;
    }

    public bool TryGetValue(out TErr? value)
    {
        value = this.err;
        return type is ResultType.Err;
    }
}
