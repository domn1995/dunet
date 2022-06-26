namespace Dunet.Test.UnionRecord;

public class GenerationTests : UnionRecordTests
{
    [Fact]
    public void UnionTypeMayHaveNoMembers()
    {
        var programCs =
            @"
using Dunet;

QueryState loading = new QueryState.Loading();
QueryState success = new QueryState.Success();
QueryState error = new QueryState.Error();

[Union]
public partial record QueryState
{
    partial record Loading();
    partial record Success();
    partial record Error();
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
