namespace Dunet.Test.UnionGeneration;

public sealed class MatchSpecificUnionValueWithStateTests
{
    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 1d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 5.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 1d)]
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
                double state = 2d;
                return shape.MatchCircle(
                    state,
                    static (s, circle) => s + 3.14 * circle.Radius * circle.Radius,
                    static s => -1 + s
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
        actualArea.Should().BeApproximately(expectedArea, 0.0000000001d);
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 1d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 1d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 6d)]
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
                double state = 2d;
                shape.MatchTriangle(
                    state,
                    (s, triangle) => { value = s + 0.5 * triangle.Base * triangle.Height; },
                    s => { value = -1 + s; }
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
        actualArea.Should().BeApproximately(expectedArea, 0.0000000001d);
    }
}
