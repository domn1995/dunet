using System.Diagnostics;

namespace StructPrototype;

public partial record struct Result<TErr, TOk>
{
    public record struct Ok(TOk Value);

    public record struct Err(TErr Error);
}

public partial record struct Result<TErr, TOk>
{
    private enum ResultType : byte
    {
        Err,
        Ok,
    };

    private ResultType type;

    private Ok ok;
    private Err err;

    public static implicit operator Result<TErr, TOk>(TOk value) => Prelude.Ok(value);

    public static implicit operator Result<TErr, TOk>(TErr error) => Prelude.Err(error);

    public TOut Match<TOut>(Func<Err, TOut> err, Func<Ok, TOut> ok) =>
        type switch
        {
            ResultType.Err => err(this.err),
            ResultType.Ok => ok(this.ok),
            var unionType
                => throw new UnreachableException(
                    $"Matched an unreachable union type: {unionType}"
                ),
        };

    public static class Prelude
    {
        public static Result<TErr, TOk> Err(TErr err) =>
            new() { type = ResultType.Err, err = new Err(err) };

        public static Result<TErr, TOk> Ok(TOk value) =>
            new() { type = ResultType.Ok, ok = new Ok(value) };
    }
}

public static class ResultPrelude
{
    public static Result<TErr, TOk> Ok<TErr, TOk>(TOk value) => Result<TErr, TOk>.Prelude.Ok(value);

    public static Result<TErr, TOk> Err<TErr, TOk>(TErr error) =>
        Result<TErr, TOk>.Prelude.Err(error);
}
