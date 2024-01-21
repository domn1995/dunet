namespace Dunet.Test.UnionGeneration;

public sealed class MatchSpecificUnionValueTests
{
    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", -1d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", -1d)]
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
                    () => -1
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
        var result = Compiler.Compile(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", -1d)]
    [InlineData("Shape shape = new Shape.Circle(1);", -1d)]
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
                double value = 0d;
                {{shapeDeclaration}}
                shape.MatchTriangle(
                    triangle => { value = 0.5 * triangle.Base * triangle.Height; },
                    () => { value = -1; }
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
        var result = Compiler.Compile(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
