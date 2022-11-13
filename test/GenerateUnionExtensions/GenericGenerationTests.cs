using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class GenericGenerationTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(1)", 1)]
    [InlineData("Task", "new Option<int>.None()", 0)]
    [InlineData("ValueTask", "new Option<int>.None()", 0)]
    public async Task SupportsAsyncMatchFunctionsForUnionsWithSingleTypeParameter(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var optionCs =
            @"
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

async static Task<int> GetValueAsync() =>
    await GetOptionAsync().MatchAsync(some => some.Value, none => 0);

async static {taskType}<Option<int>> GetOptionAsync() =>
    await Task.FromResult({optionDeclaration});
";

        // Act.
        var result = Compile.ToAssembly(optionCs, programCs);
        var value = await result.Assembly!.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(1)", 1)]
    [InlineData("Task", "new Option<int>.None()", 0)]
    [InlineData("ValueTask", "new Option<int>.None()", 0)]
    public async Task SupportsAsyncMatchActionsForUnionsWithSingleTypeParameter(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var optionCs =
            @"
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

async static Task<int> GetValueAsync()
{{
    var value = 0;
    await GetOptionAsync().MatchAsync(some => value = some.Value, none => value = 0);
    return value;
}}

async static {taskType}<Option<int>> GetOptionAsync() =>
    await Task.FromResult({optionDeclaration});
";

        // Act.
        var result = Compile.ToAssembly(optionCs, programCs);
        var value = await result.Assembly!.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
