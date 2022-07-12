namespace Dunet.Integration.GenerateUnionRecord;

using Result = Unions.Result<Exception, string>;

public class ImplicitConversion
{
    [Fact]
    public void Failure()
    {
        const string expectedMessage = "Boom!";
        var result = new Exception(expectedMessage);
        var actualMessage = GetResultMessage(result);
        actualMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void Success()
    {
        const string expectedMessage = "Success!";
        var actualMessage = GetResultMessage(expectedMessage);
        actualMessage.Should().Be(expectedMessage);
    }

    private static string GetResultMessage(Result result) =>
        result.Match(success => success.Value, failure => failure.Error.Message);
}
