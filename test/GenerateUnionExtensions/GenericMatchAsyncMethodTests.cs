using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class GenericMatchAsyncMethodTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(2)", 2)]
    [InlineData("Task", "new Option<int>.None()", 0)]
    [InlineData("ValueTask", "new Option<int>.None()", 0)]
    public async Task GenericMatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        const string optionCs =
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

async static {taskType}<Option<int>> GetOptionAsync()
{{
    await Task.Delay(0);
    return {optionDeclaration};
}}

async static Task<int> GetValueAsync() =>
    await GetOptionAsync()
        .MatchAsync(some => some.Value, none => 0);
";
        // Act.
        var result = Compile.ToAssembly(optionCs, programCs);
        var value = await result.Assembly!.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("ValueTask", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("Task", "new Result<string, double>.Failure(\"Error!\")", "Error!")]
    [InlineData("ValueTask", "new Result<string, double>.Failure(\"Error!\")", "Error!")]
    public async Task MultiGenericMatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string optionDeclaration,
        string expectedValue
    )
    {
        // Arrange.
        const string resultCs =
            @"
using Dunet;

namespace GenericsTest;

[Union]
partial record Result<TFailure, TSuccess>
{
    partial record Success(TSuccess Value);
    partial record Failure(TFailure Error);
}";

        var programCs =
            @$"
using System;
using System.Threading.Tasks;
using GenericsTest;

async static {taskType}<Result<string, double>> GetResultAsync()
{{
    await Task.Delay(0);
    return {optionDeclaration};
}}

async static Task<string> GetValueAsync() =>
    await GetResultAsync()
        .MatchAsync(success => success.Value.ToString(), failure => failure.Error);
";

        // Act.
        var result = Compile.ToAssembly(resultCs, programCs);
        var value = await result.Assembly!.ExecuteStaticAsyncMethod<string>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
