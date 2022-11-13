﻿namespace Dunet.Test.GenerateUnionExtensions;

public class GenerationTests : UnionRecordTests
{
    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void CanUseMatchAsyncOnAsyncMethodsThatReturnUnions(string taskType)
    {
        // Arrange.
        const string shapeCs =
            @"
using Dunet;

namespace Shapes;

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";

        var programCs =
            @$"
using System.Threading.Tasks;
using Shapes;

var area = await GetShapeAsync()
    .MatchAsync(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

async static {taskType}<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return new Shape.Rectangle(3, 4);
}}";
        // Act.
        var result = Compile.ToAssembly(shapeCs, programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void CanUseMatchAsyncWithActionsOnAsyncMethodsThatReturnUnions(string taskType)
    {
        // Arrange.
        const string shapeCs =
            @"
using Dunet;

namespace Shapes;

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}";

        var programCs =
            @$"
using System.Threading.Tasks;
using Shapes;

await GetShapeAsync()
    .MatchAsync(
        circle => DoNothing(),
        rectangle => DoNothing(),
        triangle => DoNothing()
    );

void DoNothing() {{ }}

async static {taskType}<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return new Shape.Rectangle(3, 4);
}}";
        // Act.
        var result = Compile.ToAssembly(shapeCs, programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void MatchAsyncMethodsAreNotGeneratedForUnionsWithNoNamespace(string taskType)
    {
        // Arrange.
        var source =
            @$"
using System.Threading.Tasks;
using Dunet;

var area = await GetShapeAsync()
    .MatchAsync(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

async static {taskType}<Shape> GetShapeAsync()
{{
    await Task.Delay(0);
    return new Shape.Rectangle(3, 4);
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
        var errorMessages = result.CompilationErrors.Select(error => error.GetMessage());

        // Assert.
        errorMessages
            .Should()
            .HaveCount(1)
            .And.Contain(
                $"'{taskType}<Shape>' does not contain a definition for 'MatchAsync' and no accessible extension method "
                    + $"'MatchAsync' accepting a first argument of type '{taskType}<Shape>' could be found (are you missing a "
                    + "using directive or an assembly reference?)"
            );
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void MatchAsyncMethodsAreNotGeneratedForUnionsWithNoMembers(string taskType)
    {
        // Arrange.
        var emptyCs =
            @"
using Dunet;

namespace EmptyTest;

[Union]
partial record Empty;";

        var source =
            @$"
using System.Threading.Tasks;
using EmptyTest;

var empty = await GetEmptyAsync()
    .MatchAsync(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

async {taskType}<Empty> GetEmptyAsync() => (null as Empty)!;";

        // Act.
        var result = Compile.ToAssembly(emptyCs, source);
        var errorMessages = result.CompilationErrors.Select(error => error.GetMessage());

        // Assert.
        errorMessages
            .Should()
            .HaveCount(1)
            .And.Contain(
                $"'{taskType}<Empty>' does not contain a definition for 'MatchAsync' and no accessible extension method "
                    + $"'MatchAsync' accepting a first argument of type '{taskType}<Empty>' could be found (are you missing a "
                    + "using directive or an assembly reference?)"
            );
        result.GenerationErrors.Should().BeEmpty();
    }
}
