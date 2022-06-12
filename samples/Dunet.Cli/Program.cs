// See https://aka.ms/new-console-template for more information

using Dunet.Shapes;

var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);
var circle = new Circle(10);

var rectangleArea = Calculator.GetArea(rectangle);
var triangleArea = Calculator.GetArea(triangle);
var circleArea = Calculator.GetArea(circle);

Console.WriteLine($"Rectangle area: {rectangleArea}");
Console.WriteLine($"Triangle area: {triangleArea}");
Console.WriteLine($"Circle area: {circleArea}");

class Calculator
{
    public static double GetArea(IShape shape) =>
        shape switch
        {
            Rectangle rect => rect.Length * rect.Width,
            Circle circle => 2.0 * Math.PI * circle.Radius,
            Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
            _ => 0d,
        };
}
