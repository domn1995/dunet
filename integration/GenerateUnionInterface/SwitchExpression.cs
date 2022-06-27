using Dunet.Integration.GenerateUnionInterface.Unions;

namespace Dunet.Integration.GenerateUnionInterface;

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

    private static double GetArea(IShape shape) =>
        shape switch
        {
            Rectangle rectangle => rectangle.Length * rectangle.Width,
            Circle circle => 3.14 * circle.Radius * circle.Radius,
            Triangle triangle => triangle.Base * triangle.Height / 2,
            _ => 0,
        };
}
