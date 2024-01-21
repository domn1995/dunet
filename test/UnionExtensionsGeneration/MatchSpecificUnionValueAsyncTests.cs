namespace Dunet.Test.GenerateUnionRecordExtensions;

public sealed class MatchSpecificUnionValueAsyncTests
{
    [Theory]
    [InlineData("Task", "new Shape.Rectangle(3, 4)", -1d)]
    [InlineData("ValueTask", "new Shape.Rectangle(3, 4)", -1d)]
    [InlineData("Task", "new Shape.Circle(1)", 3.14d)]
    [InlineData("ValueTask", "new Shape.Circle(1)", 3.14d)]
    [InlineData("Task", "new Shape.Triangle(4, 2)", -1d)]
    [InlineData("ValueTask", "new Shape.Triangle(4, 2)", -1d)]
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
                    .MatchCircleAsync(
                        circle => 3.14 * circle.Radius * circle.Radius,
                        () => -1d
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
    [InlineData("Task", "new Shape.Rectangle(3, 4)", -1d)]
    [InlineData("ValueTask", "new Shape.Rectangle(3, 4)", -1d)]
    [InlineData("Task", "new Shape.Circle(1)", 3.14d)]
    [InlineData("ValueTask", "new Shape.Circle(1)", 3.14d)]
    [InlineData("Task", "new Shape.Triangle(4, 2)", -1d)]
    [InlineData("ValueTask", "new Shape.Triangle(4, 2)", -1d)]
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
                    .MatchCircleAsync(
                        circle => { value = 3.14 * circle.Radius * circle.Radius; },
                        () => { value = -1; }
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
}
