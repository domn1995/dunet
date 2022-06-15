namespace Dunet.Test;

public class SwitchExpressionTests
{
    [Fact]
    public void CanUseUnionTypesInSwitchExpression()
    {
        // Arrange.
        var source =
            @"
using Dunet;
using System;

namespace CanUseUnionTypesWithSwitchExpression;

[Union]
interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}

public static class TestClass
{
    private static readonly IShape shape = new Rectangle(3, 4);
    public static double GetArea() => shape switch
    {
        Rectangle rect => rect.Length * rect.Width,
        Circle circle => 2.0 * Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };
}";
        // Act.
        var (assembly, compilationDiagnostics, generationDiagnostics) = Compile.ToAssembly(source);
        var result = assembly.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        compilationDiagnostics.Should().BeEmpty();
        generationDiagnostics.Should().BeEmpty();
        result.Should().Be(12d);
    }
}
