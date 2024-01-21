namespace Dunet.Test.UnionGeneration;

public sealed class SwitchExpressionTests
{
    [Fact]
    public void CanUseUnionTypesInSwitchExpression()
    {
        // Arrange.
        var source = """
            using Dunet;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Shape.Rectangle r => r.Length * r.Width,
                Shape.Circle c => 3.14 * c.Radius * c.Radius,
                Shape.Triangle t => t.Base * t.Height / 2,
                _ => 0d,
            };

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

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(4, 4);", 16d)]
    [InlineData("Shape shape = new Shape.Circle(2);", 12.56d)]
    [InlineData("Shape shape = new Shape.Triangle(2, 3);", 3d)]
    public void SwitchExpressionMatchesCorrectCase(string shapeDeclaration, double expectedArea)
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            static double GetActualArea()
            {
                {{shapeDeclaration}}
                return shape switch
                {
                    Shape.Rectangle r => r.Length * r.Width,
                    Shape.Circle c => 3.14 * c.Radius * c.Radius,
                    Shape.Triangle t => t.Base * t.Height / 2,
                    _ => 0d,
                };
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
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetActualArea");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
