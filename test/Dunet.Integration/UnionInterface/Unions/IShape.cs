namespace Dunet.Integration.UnionInterface.Unions;

[Union]
interface IShape
{
    void Rectangle(double length, double width);
    void Circle(double radius);
    void Triangle(double @base, double @height);
}
