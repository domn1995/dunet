namespace Dunet.Integration.GenerateUnionRecord.Unions;

[Union]
public partial record Result<TFailure, TSuccess>
{
    public partial record Success(TSuccess Value);

    public partial record Failure(TFailure Error);
}
