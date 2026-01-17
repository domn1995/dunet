using Dunet.Integration.GenerateUnionRecord.Unions;

namespace Dunet.Integration.GenerateUnionRecord;

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

    private static string GetResultMessage(Result<Exception, string> result) =>
        result.Match(success => success.Value, failure => failure.Error.Message);
}
