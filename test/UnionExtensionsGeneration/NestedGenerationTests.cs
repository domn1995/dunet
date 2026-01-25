namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class NestedGenerationTests
{
    [Theory]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    public async Task CanUseMatchAsyncFunctionsOnMethodsThatReturnNestedUnions(
        string taskType,
        string unionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var nestedCs = """
            using Dunet;

            namespace NestedTests;

            public partial class Parent1
            {
                public partial class Parent2
                {
                    public partial class Parent3
                    {
                        [Union]
                        public partial record Nested
                        {
                            public partial record Variant1;
                            public partial record Variant2;
                        }
                    }
                }
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using NestedTests;

            #pragma warning disable CS8321 // Called by the test.
            async static Task<int> GetValueAsync() =>
                await GetNestedAsync().MatchAsync(variant1 => 1, variant2 => 2);
            #pragma warning restore CS8321

            async static {{taskType}}<Parent1.Parent2.Parent3.Nested> GetNestedAsync()
            {
                await Task.Delay(0);
                return {{unionDeclaration}};
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(nestedCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    public async Task CanUseMatchAsyncActionsOnMethodsThatReturnNestedUnions(
        string taskType,
        string unionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var nestedCs = """
            using Dunet;

            namespace NestedTests;

            public partial class Parent1
            {
                public partial class Parent2
                {
                    public partial class Parent3
                    {
                        [Union]
                        public partial record Nested
                        {
                            public partial record Variant1;
                            public partial record Variant2;
                        }
                    }
                }
            }
            """;
        var programCs = $$"""
            using System.Threading.Tasks;
            using NestedTests;

            #pragma warning disable CS8321 // Called by the test.
            async static Task<int> GetValueAsync()
            {
                var value = 0;
                await GetNestedAsync()
                    .MatchAsync(
                        variant1 => { value = 1; },
                        variant2 => { value = 2; }
                    );
                return value;
            }
            #pragma warning restore CS8321

            async static {{taskType}}<Parent1.Parent2.Parent3.Nested> GetNestedAsync()
            {
                await Task.Delay(0);
                return {{unionDeclaration}};
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(nestedCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
