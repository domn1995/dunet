using Dunet.Integration.GenerateUnionRecord.Unions;
using static Dunet.Integration.GenerateUnionRecord.Unions.Shape;

namespace Dunet.Integration.GenerateUnionRecord;

public class SwitchExpression
{
    [Fact]
    public void Rectangle()
    {
        var shape = new Rectangle(3, 4);
        var area = GetArea(shape);
        area.Should().Be(12);
    }

    [Fact]
    public void Circle()
    {
        var shape = new Circle(2);
        var area = GetArea(shape);
        area.Should().Be(12.56);
    }

    [Fact]
    public void Triangle()
    {
        var shape = new Triangle(6, 3);
        var area = GetArea(shape);
        area.Should().Be(9);
    }

    private static double GetArea(Shape shape) =>
        shape switch
        {
            Rectangle(var length, var width) => length * width,
            Circle(var radius) => 3.14 * radius * radius,
            Triangle(var @base, var height) => @base * height / 2,
        };
}
