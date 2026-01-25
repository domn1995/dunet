namespace Dunet.Test.UnionGeneration;

public sealed class MatchMethodWithStateTests
{
    [Fact]
    public async Task CanUseUnionTypesInDedicatedMatchMethod()
    {
        // Arrange.
        var source = """
            using Dunet;

            Shape shape = new Shape.Rectangle(3, 4);
            double state = 2d;

            var area = shape.Match(
                state,
                static (s, circle) => s + 3.14 * circle.Radius * circle.Radius,
                static (s, rectangle) => s + rectangle.Length * rectangle.Width,
                static (s, triangle) => s + triangle.Base * triangle.Height / 2
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
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 14d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 5.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 6d)]
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
                double state = 2d;
                return shape.Match(
                    state,
                    static (s, circle) => s + 3.14 * circle.Radius * circle.Radius,
                    static (s, rectangle) => s + rectangle.Length * rectangle.Width,
                    static (s, triangle) => s + triangle.Base * triangle.Height / 2
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
        actualArea.Should().BeApproximately(expectedArea, 0.0000000001d);
    }

    [Theory]
    [InlineData("Keyword keyword = new Keyword.New();", "string state = \"new\";", "new")]
    [InlineData("Keyword keyword = new Keyword.Base();", "string state = \"base\";", "base")]
    [InlineData("Keyword keyword = new Keyword.Null();", "string state = \"null\";", "null")]
    public async Task CanMatchOnUnionVariantsNamedAfterKeywords(
        string keywordDeclaration,
        string stateDeclaration,
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
                {{stateDeclaration}}
                return keyword.Match(
                    state,
                    static (s, @new) => s,
                    static (s, @base) => s,
                    static (s, @null) => s
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
