using System.Diagnostics;

namespace StructPrototype;

public record struct Result<TErr, TOk>
    where TErr : notnull
    where TOk : notnull
{
    private enum ResultType : byte
    {
        Err,
        Ok,
    };

    private ResultType type;

    private TOk ok;

    public static Result<TErr, TOk> NewOk(TOk value) => new() { type = ResultType.Ok, ok = value };

    public static implicit operator Result<TErr, TOk>(TOk value) => NewOk(value);

    public TOk UnwrapOk() =>
        type switch
        {
            ResultType.Ok => ok,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {ResultType.Ok} but got {actual}"
                ),
        };

    private TErr err;

    public static Result<TErr, TOk> NewErr(TErr err) => new() { type = ResultType.Err, err = err };

    public static implicit operator Result<TErr, TOk>(TErr error) => NewErr(error);

    public TErr UnwrapErr() =>
        type switch
        {
            ResultType.Err => err,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {ResultType.Err} but got {actual}"
                ),
        };

    public TOut Match<TOut>(Func<TErr, TOut> err, Func<TOk, TOut> ok) =>
        type switch
        {
            ResultType.Err => err(this.err),
            ResultType.Ok => ok(this.ok),
            var invalid
                => throw new UnreachableException($"Matched an unreachable union type: {invalid}"),
        };
}

public static class ResultPrelude
{
    public static Result<TErr, TOk> Ok<TErr, TOk>(TOk value)
        where TErr : notnull
        where TOk : notnull => Result<TErr, TOk>.NewOk(value);

    public static Result<TErr, TOk> Err<TErr, TOk>(TErr error)
        where TErr : notnull
        where TOk : notnull => Result<TErr, TOk>.NewErr(error);
}
