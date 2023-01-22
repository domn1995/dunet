using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionRecord;

public sealed class MatchSpecificUnionValueTests : UnionRecordTests
{
    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 0d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 0d)]
    public void SpecificMatchMethodCallsCorrectFunctionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source = $$"""
using Dunet;

static double GetArea()
{
    {{shapeDeclaration}}
    return shape.MatchCircle(
        circle => 3.14 * circle.Radius * circle.Radius,
        () => 0
    );
}

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}
""";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 0d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 0d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 4d)]
    public void SpecificMatchMethodCallsCorrectActionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source = $$"""
using Dunet;

static double GetArea()
{
    double value = -1d;
    {{shapeDeclaration}}
    shape.MatchTriangle(
        triangle => { value = 0.5 * triangle.Base * triangle.Height; },
        () => { value = 0; }
    );
    return value;
}

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}
""";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
