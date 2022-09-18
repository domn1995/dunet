using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionExtensions;

public class MatchAsyncMethodTests : UnionRecordTests
{
    [Fact]
    public void CanUseMatchAsyncMethodOnAsyncTaskMethods()
    {
        // Arrange.
        var source =
            @"
using System.Threading.Tasks;
using Dunet;
using Dunet.Extensions;

var area = await GetShapeAsync()
    .MatchAsync(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

async Task<Shape> GetShapeAsync()
{
    await Task.Delay(0);
    return new Shape.Rectangle(3, 4);
}

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";
        // Act.
        var result = Compile.ToAssembly(source);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Fact]
    public void CanUseMatchAsyncMethodOnAsyncValueTaskMethods()
    {
        // Arrange.
        var source =
            @"
using System.Threading.Tasks;
using Dunet;
using Dunet.Extensions;

var area = await GetShapeAsync()
    .MatchAsync(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

async ValueTask<Shape> GetShapeAsync()
{
    await Task.Delay(0);
    return new Shape.Rectangle(3, 4);
}

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";
        // Act.
        var result = Compile.ToAssembly(source);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

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
using Dunet;
using Dunet.Extensions;

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
}}

[Union]
partial record Shape
{{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}}";
        // Act.
        var result = Compile.ToAssembly(source);
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
using Dunet;
using Dunet.Extensions;

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
}}

[Union]
partial record Shape
{{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}}";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualArea = await result.Assembly!.ExecuteStaticAsyncMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
