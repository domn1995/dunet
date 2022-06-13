// See https://aka.ms/new-console-template for more information

using Dunet.Choices;
using Dunet.Shapes;

var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);
var circle = new Circle(10);

var getArea = (IShape shape) =>
    shape switch
    {
        Rectangle rect => rect.Length * rect.Width,
        Circle circle => 2.0 * Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };

var rectangleArea = getArea(rectangle);
var triangleArea = getArea(triangle);
var circleArea = getArea(circle);

Console.WriteLine($"Rectangle area: {rectangleArea}");
Console.WriteLine($"Triangle area: {triangleArea}");
Console.WriteLine($"Circle area: {circleArea}");

var choice = GetChoice();

if (choice is Yes)
{
    Console.WriteLine("YES!!!");
}

if (choice is No)
{
    Console.WriteLine("NO!!!");
}

static IChoice GetChoice() =>
    Console.ReadLine() switch
    {
        "yes" => new Yes(),
        _ => new No(),
    };
