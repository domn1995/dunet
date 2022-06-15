using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

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

namespace Test;

[Union]
interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}

public static class Program
{
    private static readonly IShape shape = new Rectangle(3, 4);

    public static void Main()
    {
        GetArea();
    }

    public static double GetArea() => shape switch
    {
        Rectangle rect => rect.Length * rect.Width,
        Circle circle => 3.14 * circle.Radius * circle.Radius,
        Triangle triangle => triangle.Base * triangle.Height / 2,
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
