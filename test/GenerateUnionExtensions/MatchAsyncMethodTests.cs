using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class MatchAsyncMethodTests : UnionRecordTests
{
    private const string shapeCs =
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

    [Theory]
    [InlineData("new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("new Shape.Circle(1);", 3.14d)]
    [InlineData("new Shape.Triangle(4, 2);", 4d)]
    public async Task MatchAsyncTaskMethodCallsCorrectFunctionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source =
            @$"
using System.Threading.Tasks;
using Shapes;

async static Task<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return {shapeDeclaration};
}};

async static Task<double> GetArea()
{{
    return await GetShapeAsync()
        .MatchAsync(
            circle => 3.14 * circle.Radius * circle.Radius,
            rectangle => rectangle.Length * rectangle.Width,
            triangle => triangle.Base * triangle.Height / 2
        );
}}";
        // Act.
        var result = Compile.ToAssembly(shapeCs, source);
        var actualArea = await result.Assembly!.ExecuteStaticAsyncMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("new Shape.Circle(1);", 3.14d)]
    [InlineData("new Shape.Triangle(4, 2);", 4d)]
    public async Task MatchAsyncValueTaskMethodCallsCorrectFunctionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source =
            @$"
using System.Threading.Tasks;
using Shapes;

async static ValueTask<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return {shapeDeclaration};
}};

async static Task<double> GetArea()
{{
    return await GetShapeAsync()
        .MatchAsync(
            circle => 3.14 * circle.Radius * circle.Radius,
            rectangle => rectangle.Length * rectangle.Width,
            triangle => triangle.Base * triangle.Height / 2
        );
}}";
        // Act.
        var result = Compile.ToAssembly(shapeCs, source);
        var actualArea = await result.Assembly!.ExecuteStaticAsyncMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
