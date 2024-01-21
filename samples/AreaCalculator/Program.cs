using Dunet;
using static Shape;

var circle = new Circle(10);
var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);

Console.WriteLine($"Circle area: {GetArea(circle)}");
Console.WriteLine($"Rectangle area: {GetArea(rectangle)}");
Console.WriteLine($"Triangle area: {GetArea(triangle)}");

static double GetArea(Shape shape) =>
    shape.Match(
        static circle => Math.PI * circle.Radius * circle.Radius,
        static rectangle => rectangle.Length * rectangle.Width,
        static triangle => triangle.Base * triangle.Height / 2
    );

[Union]
partial record Shape
{
    partial record Circle(double Radius);

    partial record Rectangle(double Length, double Width);

    partial record Triangle(double Base, double Height);
}
