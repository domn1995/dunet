namespace Dunet.Test.UnionGeneration;

/// <summary>
/// Tests the correctness of match methods that don't return anything.
/// </summary>
public sealed class ActionMatchMethodTests
{
    [Fact]
    public void CanUseUnionTypesInActionMatchMethod()
    {
        // Arrange.
        var source = """
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
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 4d)]
    public void MatchMethodCallsCorrectActionArgument(string shapeDeclaration, double expectedArea)
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            static double GetArea()
            {
                double value = 0d;
                {{shapeDeclaration}}
                shape.Match(
                    circle => { value = 3.14 * circle.Radius * circle.Radius; },
                    rectangle => { value = rectangle.Length * rectangle.Width; },
                    triangle => { value = triangle.Base * triangle.Height / 2; }
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

    [Theory]
    [InlineData(1, 2, "0.5")]
    [InlineData(1, 0, "Error: division by zero.")]
    public void GenericMatchMethodCallsCorrectActionArgument(
        int dividend,
        int divisor,
        string expectedOutput
    )
    {
        var programCs = $$"""
            using Dunet;
            using System.Globalization;

            static string GetResult()
            {
                var value = "";
                Divide().Match(
                    some => value = some.Value.ToString(CultureInfo.InvariantCulture),
                    none => value = "Error: division by zero."
                );
                return value;
            };

            static Option<double> Divide()
            {
                var dividend = {{dividend}};
                var divisor = {{divisor}};

                if (divisor is 0)
                {
                    return new Option<double>.None();
                }

                return new Option<double>.Some((double)dividend / divisor);
            }

            [Union]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;

        // Act.
        var result = Compiler.Compile(programCs);
        var actualArea = result.Assembly?.ExecuteStaticMethod<string>("GetResult");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("""Success("Successful!")""", "Successful!")]
    [InlineData("""Failure(new Exception("Failure!"))""", "Failure!")]
    public void MultiGenericMatchMethodCallsCorrectActionArgument(
        string resultRecord,
        string expectedMessage
    )
    {
        var programCs = $$"""
            using System;
            using Dunet;

            static Result<Exception, string> DoWork() => new Result<Exception, string>.{{resultRecord}};

            static string GetActualMessage()
            {
                var value = "";
                DoWork().Match(
                    success => value = success.Value,
                    failure => value = failure.Error.Message
                );
                return value;
            }

            [Union]
            partial record Result<TFailure, TSuccess>
            {
                partial record Success(TSuccess Value);
                partial record Failure(TFailure Error);
            }
            """;

        // Act.
        var result = Compiler.Compile(programCs);
        var actualMessage = result.Assembly?.ExecuteStaticMethod<string>("GetActualMessage");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
        actualMessage.Should().Be(expectedMessage);
    }
}
