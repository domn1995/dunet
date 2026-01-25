namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class GenericGenerationTests
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
        var optionCs = """
            using Dunet;

            namespace GenericsTest;

            [Union]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;
        var programCs = $"""
            using System.Threading.Tasks;
            using GenericsTest;

            #pragma warning disable CS8321 // Called by the test only.
            async static Task<int> GetValueAsync() =>
                await GetOptionAsync().MatchAsync(some => some.Value, none => 0);
            #pragma warning restore CS8321 // Called by test.

            async static {taskType}<Option<int>> GetOptionAsync() =>
                await Task.FromResult({optionDeclaration});
            """;

        // Act.
        var result = await Compiler.CompileAsync(optionCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Option<int>.Some(1)", 1)]
    [InlineData("ValueTask", "new Option<int>.Some(1)", 1)]
    [InlineData("Task", "new Option<int>.None()", -1)]
    [InlineData("ValueTask", "new Option<int>.None()", -1)]
    public async Task SupportsAsyncMatchActionsForUnionsWithSingleTypeParameter(
        string taskType,
        string optionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var optionCs = """
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

            #pragma warning disable CS8321 // Called by the test only.
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
            #pragma warning restore CS8321 // Called by test.

            async static {{taskType}}<Option<int>> GetOptionAsync() =>
                await Task.FromResult({{optionDeclaration}});
            """;

        // Act.
        var result = await Compiler.CompileAsync(optionCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
