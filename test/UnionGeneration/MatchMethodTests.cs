namespace Dunet.Test.UnionGeneration;

public sealed class MatchMethodTests
{
    [Fact]
    public async Task CanUseUnionTypesInDedicatedMatchMethod()
    {
        // Arrange.
        var source = """
            using Dunet;

            Shape shape = new Shape.Rectangle(3, 4);

            var area = shape.Match(
                circle => 3.14 * circle.Radius * circle.Radius,
                rectangle => rectangle.Length * rectangle.Width,
                triangle => triangle.Base * triangle.Height / 2
            );

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 4d)]
    public async Task MatchMethodCallsCorrectFunctionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            #pragma warning disable CS8321 // Called by the test.
            static double GetArea()
            {
                {{shapeDeclaration}}
                return shape.Match(
                    circle => 3.14 * circle.Radius * circle.Radius,
                    rectangle => rectangle.Length * rectangle.Width,
                    triangle => triangle.Base * triangle.Height / 2
                );
            }
            #pragma warning restore CS8321

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Keyword keyword = new Keyword.New();", "new")]
    [InlineData("Keyword keyword = new Keyword.Base();", "base")]
    [InlineData("Keyword keyword = new Keyword.Null();", "null")]
    public async Task CanMatchOnUnionVariantsNamedAfterKeywords(
        string keywordDeclaration,
        string expectedKeyword
    )
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            #pragma warning disable CS8321 // Called by the test.
            static string GetKeyword()
            {
                {{keywordDeclaration}}
                return keyword.Match(
                    @new => "new",
                    @base => "base",
                    @null => "null"
                );
            }
            #pragma warning restore CS8321

            [Union]
            partial record Keyword
            {
                partial record New;
                partial record Base;
                partial record Null;
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);
        var actualKeyword = result.Assembly?.ExecuteStaticMethod<string>("GetKeyword");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        actualKeyword.Should().Be(expectedKeyword);
    }
}
