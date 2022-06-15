namespace Dunet.Test;

public class NamespaceTests
{
    [Fact]
    public void CanReferenceUnionTypesFromSeparateNamespace()
    {
        // Arrange.
        var iShapeCs =
            @"
using Dunet;

namespace Shapes;

[Union]
interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}
";
        var programCs =
            @"
using System;
using Shapes;

namespace Test;

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
        Circle circle => 2.0 * Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };
}";
        // Act.
        var (assembly, compilationDiagnostics, generationDiagnostics) = Compile.ToAssembly(
            iShapeCs,
            programCs
        );
        var result = assembly.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        compilationDiagnostics.Should().BeEmpty();
        generationDiagnostics.Should().BeEmpty();
        result.Should().Be(12d);
    }

    [Fact]
    public void CanUseUnionTypesInSameNamespace()
    {
        var programCs =
            @"
using System;
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
        Circle circle => 2.0 * Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };
}";
        // Act.
        var (assembly, compilationDiagnostics, generationDiagnostics) = Compile.ToAssembly(
            programCs
        );
        var result = assembly.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        compilationDiagnostics.Should().BeEmpty();
        generationDiagnostics.Should().BeEmpty();
        result.Should().Be(12d);
    }
}
