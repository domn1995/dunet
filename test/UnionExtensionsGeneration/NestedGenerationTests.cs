namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class NestedGenerationTests
{
    [Theory]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    public void CanUseMatchAsyncFunctionsOnMethodsThatReturnNestedUnions(
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

            async static Task<int> GetValueAsync() =>
                await GetNestedAsync().MatchAsync(variant1 => 1, variant2 => 2);

            async static {{taskType}}<Parent1.Parent2.Parent3.Nested> GetNestedAsync()
            {
                await Task.Delay(0);
                return {{unionDeclaration}};
            }
            """;

        // Act.
        var result = Compiler.Compile(nestedCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant1()", 1)]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Variant2()", 2)]
    public void CanUseMatchAsyncActionsOnMethodsThatReturnNestedUnions(
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

            async static {{taskType}}<Parent1.Parent2.Parent3.Nested> GetNestedAsync()
            {
                await Task.Delay(0);
                return {{unionDeclaration}};
            }
            """;

        // Act.
        var result = Compiler.Compile(nestedCs, programCs);
        var value = result.Assembly?.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
