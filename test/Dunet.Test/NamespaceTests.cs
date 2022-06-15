using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

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
    public static void Main()
    {
        IShape circle = new Circle(3.14);
        IShape rectangle = new Rectangle(1.5, 3.5);
        IShape triangle = new Triangle(2.0, 3.0);
    }
}";
        // Act.
        var result = Compile.ToAssembly(iShapeCs, programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanUseUnionTypesInSameNamespace()
    {
        var programCs =
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
    public static void Main()
    {
        IShape circle = new Circle(3.14);
        IShape rectangle = new Rectangle(1.5, 3.5);
        IShape triangle = new Triangle(2.0, 3.0);
    }
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanUseUnionTypesInTopLevelPrograms()
    {
        var programCs =
            @"
using Dunet;

IShape circle = new Circle(3.14);
IShape rectangle = new Rectangle(1.5, 3.5);
IShape triangle = new Triangle(2.0, 3.0);

[Union]
interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
