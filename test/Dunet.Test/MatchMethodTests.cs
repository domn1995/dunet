using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test;

public class MatchMethodTests
{
    [Fact]
    public void CanUseUnionTypesInDedicatedMatchMethod()
    {
        // Arrange.
        var source =
            @"
using Dunet;

IShape shape = new Rectangle(3, 4);

var area = shape.Match(
    circle => 3.14 * circle.Radius * circle.Radius,
    rectangle => rectangle.Length * rectangle.Width,
    triangle => triangle.Base * triangle.Height / 2
);

[Union]
interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}";
        // Act.
        var result = Compile.ToAssembly(source);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }
}
