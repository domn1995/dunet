using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionInterface;

public class SwitchExpressionTests : UnionInterfaceTests
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

    [Theory]
    [InlineData("IShape shape = new Rectangle(4, 4);", 16d)]
    [InlineData("IShape shape = new Circle(2);", 12.56d)]
    [InlineData("IShape shape = new Triangle(2, 3);", 3d)]
    public void SwitchExpressionMatchesCorrectCase(string shapeDeclaration, double expectedArea)
    {
        // Arrange.
        var source =
            @$"
using Dunet;

static double GetArea()
{{
    {shapeDeclaration}
    return shape switch
    {{
        Rectangle r => r.Length * r.Width,
        Circle c => 3.14 * c.Radius * c.Radius,
        Triangle t => t.Base * t.Height / 2,
        _ => 0d,
    }};
}}

[Union]
interface IShape
{{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}}";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualArea = result.Assembly.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }
}
