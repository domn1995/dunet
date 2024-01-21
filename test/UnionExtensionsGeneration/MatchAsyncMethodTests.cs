namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class MatchAsyncMethodTests
{
    [Theory]
    [InlineData("Task", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("ValueTask", "new Shape.Rectangle(3, 4)", 12d)]
    [InlineData("Task", "new Shape.Circle(1)", 3.14d)]
    [InlineData("ValueTask", "new Shape.Circle(1)", 3.14d)]
    [InlineData("Task", "new Shape.Triangle(4, 2)", 4d)]
    [InlineData("ValueTask", "new Shape.Triangle(4, 2)", 4d)]
    public void MatchAsyncCallsCorrectFunctionArgument(
        string taskType,
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        const string shapeCs = """
            using Dunet;

            namespace Shapes;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using Shapes;

            async static {{taskType}}<Shape> GetShapeAsync()
            {
                await Task.Delay(0);
                return {{shapeDeclaration}};
            };

            async static Task<double> GetAreaAsync() =>
                await GetShapeAsync()
                    .MatchAsync(
                        circle => 3.14 * circle.Radius * circle.Radius,
                        rectangle => rectangle.Length * rectangle.Width,
                        triangle => triangle.Base * triangle.Height / 2
                    );
            """;

        // Act.
        var result = Compiler.Compile(shapeCs, programCs);
        var actualArea = result.Assembly?.ExecuteStaticAsyncMethod<double>("GetAreaAsync");

        // Assert.
        using var scope = new AssertionScope();
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
    public void MatchAsyncCallsCorrectActionArgument(
        string taskType,
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        const string shapeCs = """
            using Dunet;

            namespace Shapes;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using Shapes;

            async static {{taskType}}<Shape> GetShapeAsync()
            {
                await Task.Delay(0);
                return {{shapeDeclaration}};
            };

            async static Task<double> GetAreaAsync()
            {
                var value = 0d;
                await GetShapeAsync()
                    .MatchAsync(
                        circle => { value = 3.14 * circle.Radius * circle.Radius; },
                        rectangle => { value = rectangle.Length * rectangle.Width; },
                        triangle => { value = triangle.Base * triangle.Height / 2; }
                    );
                return value;
            }
            """;

        // Act.
        var result = Compiler.Compile(shapeCs, programCs);
        var actualArea = result.Assembly?.ExecuteStaticAsyncMethod<double>("GetAreaAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Task", "new Keyword.New()", "new")]
    [InlineData("ValueTask", "new Keyword.New()", "new")]
    [InlineData("Task", "new Keyword.Base()", "base")]
    [InlineData("ValueTask", "new Keyword.Base()", "base")]
    [InlineData("Task", "new Keyword.Null()", "null")]
    [InlineData("ValueTask", "new Keyword.Null()", "null")]
    public void CanMatchAsyncOnUnionVariantsNamedAfterKeywords(
        string taskType,
        string keywordDeclaration,
        string expectedKeyword
    )
    {
        // Arrange.
        const string keywordCs = """
            using Dunet;

            namespace Keywords;

            [Union]
            partial record Keyword
            {
                partial record New;
                partial record Base;
                partial record Null;
            }
            """;

        var programCs = $$"""
            using System.Threading.Tasks;
            using Keywords;

            async static {{taskType}}<Keyword> GetKeywordAsync()
            {
                await Task.Delay(0);
                return {{keywordDeclaration}};
            };

            async static Task<string> GetValueAsync()
            {
                var keyword = "";
                await GetKeywordAsync()
                    .MatchAsync(
                        @new => keyword = "new",
                        @base => keyword = "base",
                        @null => keyword = "null"
                    );
                return keyword;
            }
            """;

        // Act.
        var result = Compiler.Compile(keywordCs, programCs);
        var actualKeyword = result.Assembly?.ExecuteStaticAsyncMethod<string>("GetValueAsync");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualKeyword.Should().Be(expectedKeyword);
    }
}
