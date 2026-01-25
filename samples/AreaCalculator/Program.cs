using Dunet;
using static Shape;

var circle = new Circle(10);
var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);

Console.WriteLine($"Circle area: {GetArea(circle)}");
Console.WriteLine($"Rectangle area: {GetArea(rectangle)}");
Console.WriteLine($"Triangle area: {GetArea(triangle)}");

static double GetArea(Shape shape) =>
    shape switch
    {
        Circle(var radius) => Math.PI * radius * radius,
        Rectangle(var length, var width) => length * width,
        Triangle(var @base, var height) => @base * height / 2,
    };

[Union]
partial record Shape
{
    partial record Circle(double Radius);

    partial record Rectangle(double Length, double Width);

    partial record Triangle(double Base, double Height);
}
