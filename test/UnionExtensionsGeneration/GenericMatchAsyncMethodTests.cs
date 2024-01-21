namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class GenericMatchAsyncMethodTests
{
    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(2)", 2)]
    [InlineData("Task", "new Option<int>.None()", -1)]
    [InlineData("ValueTask", "new Option<int>.None()", -1)]
    public void GenericMatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        const string optionCs = """
            using Dunet;

            namespace GenericsTest;

            [Union]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using GenericsTest;

            async static {{taskType}}<Option<int>> GetOptionAsync()
            {
                await Task.Delay(0);
                return {{optionDeclaration}};
            }

            async static Task<int> GetValueAsync() =>
                await GetOptionAsync()
                    .MatchAsync(some => some.Value, none => -1);
            """;
        // Act.
        var result = Compiler.Compile(optionCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(2)", 2)]
    [InlineData("Task", "new Option<int>.None()", -1)]
    [InlineData("ValueTask", "new Option<int>.None()", -1)]
    public void GenericMatchAsyncCallsCorrectActionArgument(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        const string optionCs = """
            using Dunet;

            namespace GenericsTest;

            [Union]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using GenericsTest;

            async static {{taskType}}<Option<int>> GetOptionAsync()
            {
                await Task.Delay(0);
                return {{optionDeclaration}};
            }

            async static Task<int> GetValueAsync()
            {
                var value = 0;
                await GetOptionAsync()
                    .MatchAsync(
                        some => { value = some.Value; },
                        none => { value = -1; }
                    );
                return value;
            }
            """;
        // Act.
        var result = Compiler.Compile(optionCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("ValueTask", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("Task", """new Result<string, double>.Failure("Error!")""", "Error!")]
    [InlineData("ValueTask", """new Result<string, double>.Failure("Error!")""", "Error!")]
    public void MultiGenericMatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string resultDeclaration,
        string expectedValue
    )
    {
        // Arrange.
        const string resultCs = """
            using Dunet;

            namespace GenericsTest;

            [Union]
            partial record Result<TFailure, TSuccess>
            {
                partial record Success(TSuccess Value);
                partial record Failure(TFailure Error);
            }
            """;

        var programCs = $$"""
            using System;
            using System.Threading.Tasks;
            using GenericsTest;

            async static {{taskType}}<Result<string, double>> GetResultAsync()
            {
                await Task.Delay(0);
                return {{resultDeclaration}};
            }

            async static Task<string> GetValueAsync() =>
                await GetResultAsync()
                    .MatchAsync(success => success.Value.ToString(), failure => failure.Error);
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<string>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("ValueTask", "new Result<string, double>.Success(1d)", "1")]
    [InlineData("Task", """new Result<string, double>.Failure("Error!")""", "Error!")]
    [InlineData("ValueTask", """new Result<string, double>.Failure("Error!")""", "Error!")]
    public void MultiGenericMatchAsyncCallsCorrectActionArgument(
        string taskType,
        string resultDeclaration,
        string expectedValue
    )
    {
        // Arrange.
        const string resultCs = """
            using Dunet;

            namespace GenericsTest;

            [Union]
            partial record Result<TFailure, TSuccess>
            {
                partial record Success(TSuccess Value);
                partial record Failure(TFailure Error);
            }
            """;

        var programCs = $$"""
            using System;
            using System.Threading.Tasks;
            using GenericsTest;

            async static {{taskType}}<Result<string, double>> GetResultAsync()
            {
                await Task.Delay(0);
                return {{resultDeclaration}};
            }

            async static Task<string> GetValueAsync()
            {
                var value = "";
                await GetResultAsync()
                    .MatchAsync(
                        success => { value = success.Value.ToString(); },
                        failure => { value = failure.Error; }
                    );
                return value;
            }
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<string>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
