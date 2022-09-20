namespace Dunet.Test.GenerateUnionExtensions;

public class GenericGenerationTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void SupportsUnionsWithSingleTypeParameter(string taskType)
    {
        // Arrange.
        var optionCs = @"
using Dunet;

namespace GenericsTest;

[Union]
partial record Option<T>
{
    partial record Some(T Value);
    partial record None();
}";
        var programCs =
            @$"
using System.Threading.Tasks;
using GenericsTest;

async static Task<Option<int>> await GetOptionAsync()
    .MatchAsync(some => some.Value, none => 0);

async static {taskType}<Option<int>> GetOptionAsync() =>
    await Task.FromResult(new Option<int>.Some(1));
";

        // Act.
        var result = Compile.ToAssembly(optionCs, programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}

