using System.Diagnostics;

namespace StructPrototype;

public partial record struct Result<TErr, TOk>
{
    public record struct Ok(TOk Value);

    public record struct Err(TErr Error);
}

public readonly partial record struct Result<TErr, TOk>
{
    private enum ResultType : byte
    {
        Err,
        Ok,
    };

    private readonly ResultType type;

    private readonly Ok ok;
    private readonly Err err;

    public static implicit operator Result<TErr, TOk>(TOk value) => NewOk(value);

    public static implicit operator Result<TErr, TOk>(TErr error) => NewErr(error);

    private Result(ResultType type, Ok ok) => (this.type, this.ok) = (type, ok);

    private Result(ResultType type, Err err) => (this.type, this.err) = (type, err);

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

    public static Result<TErr, TOk> NewOk(TOk value) => new(ResultType.Ok, new Ok(value));

    public static Result<TErr, TOk> NewErr(TErr error) => new(ResultType.Ok, new Err(error));
}
