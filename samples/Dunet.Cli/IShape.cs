namespace Dunet.Shapes;

[Union]
public interface IShape
{
    void Circle(double radius);
    void Rectangle(double length, double width);
    void Triangle(double @base, double height);
}
