// See https://aka.ms/new-console-template for more information

using Dunet.Shapes;

var circle = new Shape.Circle(10);
var rectangle = new Shape.Rectangle(10, 10);
var triangle = new Shape.Triangle(10, 10);

var getArea = (Shape shape) =>
    shape.Match(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );

var circleArea = getArea(circle);
var rectangleArea = getArea(rectangle);
var triangleArea = getArea(triangle);

Console.WriteLine($"Circle area: {circleArea}");
Console.WriteLine($"Rectangle area: {rectangleArea}");
Console.WriteLine($"Triangle area: {triangleArea}");
