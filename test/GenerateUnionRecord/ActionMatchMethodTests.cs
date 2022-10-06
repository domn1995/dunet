using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionRecord;

public class ActionMatchMethodTests : UnionRecordTests
{
    [Fact]
    public void CanUseUnionTypesInActionMatchMethod()
    {
        // Arrange.
        var source =
            @"
using Dunet;

Shape shape = new Shape.Rectangle(3, 4);

shape.Match(
    circle => DoNothing(),
    rectangle => DoNothing(),
    triangle => DoNothing()
);

void DoNothing() { }

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
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 4d)]
    public void MatchMethodCallsCorrectActionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source =
            @$"
using Dunet;

static double GetArea()
{{
    double value = 0d;
    {shapeDeclaration}
    shape.Match(
        circle => value = 3.14 * circle.Radius * circle.Radius,
        rectangle => value = rectangle.Length * rectangle.Width,
        triangle => value = triangle.Base * triangle.Height / 2
    );
    return value;
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
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
