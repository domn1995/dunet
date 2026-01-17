namespace Dunet.Integration.GenerateUnionRecord.Unions;

[Union]
public partial record Result<TError, TSuccess>
{
    public partial record Success(TSuccess Value);

    public partial record Failure(TError Error);
}

[Union]
public partial record Result<TSuccess>
{
    public partial record Success(TSuccess Value);

    public partial record Failure();
}

public static class Result
{
    public static Result<Exception, string> Success(string value) =>
        new Result<Exception, string>.Success(value);

    public static Result<Exception, string> Failure(Exception exception) =>
        new Result<Exception, string>.Failure(exception);
}
