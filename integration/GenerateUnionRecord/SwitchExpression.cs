using Dunet.Integration.GenerateUnionRecord.Unions;

namespace Dunet.Integration.GenerateUnionRecord;

public class SwitchExpression
{
    [Fact]
    public void Rectangle()
    {
        var shape = new Shape.Rectangle(3, 4);
        var area = GetArea(shape);
        area.Should().Be(12);
    }

    [Fact]
    public void Circle()
    {
        var shape = new Shape.Circle(2);
        var area = GetArea(shape);
        area.Should().Be(12.56);
    }

    [Fact]
    public void Triangle()
    {
        var shape = new Shape.Triangle(6, 3);
        var area = GetArea(shape);
        area.Should().Be(9);
    }

    private static double GetArea(Shape shape) =>
        shape switch
        {
            Shape.Rectangle rectangle => rectangle.Length * rectangle.Width,
            Shape.Circle circle => 3.14 * circle.Radius * circle.Radius,
            Shape.Triangle triangle => triangle.Base * triangle.Height / 2,
            _ => 0,
        };
}
