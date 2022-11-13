using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class MatchAsyncMethodTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("ValueTask", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("Task", "new Shape.Circle(1)", 3.14d)]
    [InlineData("ValueTask", "new Shape.Circle(1)", 3.14d)]
    [InlineData("Task", "new Shape.Triangle(4, 2)", 4d)]
    [InlineData("ValueTask", "new Shape.Triangle(4, 2)", 4d)]
    public async Task MatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        const string shapeCs =
            @"
using Dunet;

namespace Shapes;

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";

        var programCs =
            @$"
using System.Threading.Tasks;
using Shapes;

async static {taskType}<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return {shapeDeclaration};
}};

async static Task<double> GetAreaAsync() =>
    await GetShapeAsync()
        .MatchAsync(
            circle => 3.14 * circle.Radius * circle.Radius,
            rectangle => rectangle.Length * rectangle.Width,
            triangle => triangle.Base * triangle.Height / 2
        );";

        // Act.
        var result = Compile.ToAssembly(shapeCs, programCs);
        var actualArea = await result.Assembly!.ExecuteStaticAsyncMethod<double>("GetAreaAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Task", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("ValueTask", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("Task", "new Shape.Circle(1)", 3.14d)]
    [InlineData("ValueTask", "new Shape.Circle(1)", 3.14d)]
    [InlineData("Task", "new Shape.Triangle(4, 2)", 4d)]
    [InlineData("ValueTask", "new Shape.Triangle(4, 2)", 4d)]
    public async Task MatchAsyncCallsCorrectActionArgument(
        string taskType,
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        const string shapeCs =
            @"
using Dunet;

namespace Shapes;

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";

        var programCs =
            @$"
using System.Threading.Tasks;
using Shapes;

async static {taskType}<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return {shapeDeclaration};
}};

async static Task<double> GetAreaAsync()
{{
    var value = 0d;
    await GetShapeAsync()
        .MatchAsync(
            circle => value = 3.14 * circle.Radius * circle.Radius,
            rectangle => value = rectangle.Length * rectangle.Width,
            triangle => value = triangle.Base * triangle.Height / 2
        );
    return value;
}}";

        // Act.
        var result = Compile.ToAssembly(shapeCs, programCs);
        var actualArea = await result.Assembly!.ExecuteStaticAsyncMethod<double>("GetAreaAsync");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
