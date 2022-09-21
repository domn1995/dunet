using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class NestedGenerationTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Member1()", 1)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Member1()", 1)]
    [InlineData("Task", "new Parent1.Parent2.Parent3.Nested.Member2()", 2)]
    [InlineData("ValueTask", "new Parent1.Parent2.Parent3.Nested.Member2()", 2)]
    public async Task CanUseMatchAsyncMethodsOnMethodsThatReturnNestedUnions(
        string taskType,
        string unionDeclaration,
        int expectedValue
    )
    {
        // Arrange.
        var nestedCs =
            @"
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
                public partial record Member1;
                public partial record Member2;
            }
        }
    }
};";
        var programCs =
            @$"
using System.Threading.Tasks;
using NestedTests;

async static Task<int> GetValueAsync() =>
    await GetNestedAsync().MatchAsync(member1 => 1, member2 => 2);

async static {taskType}<Parent1.Parent2.Parent3.Nested> GetNestedAsync()
{{
    await Task.Delay(0);
    return {unionDeclaration};
}}
";

        // Act.
        var result = Compile.ToAssembly(nestedCs, programCs);
        var value = await result.Assembly!.ExecuteStaticAsyncMethod<int>("GetValueAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(expectedValue);
    }
}
