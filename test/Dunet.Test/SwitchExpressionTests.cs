using Dunet.Test.Compiler;

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

IShape circle = new Circle(3.14);

var area = circle switch
{
    Rectangle r => r.Length * r.Width,
    Circle c => 3.14 * c.Radius * c.Radius,
    Triangle t => t.Base * t.Height / 2,
    _ => 0d,
};

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
