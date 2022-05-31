// See https://aka.ms/new-console-template for more information

using Dunet.Cli;

var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);
var circle = new Circle(10);

var rectangleArea = Calculator.GetArea(rectangle);
var triangleArea = Calculator.GetArea(triangle);
var circleArea = Calculator.GetArea(circle);

Console.WriteLine($"Rectangle area: {rectangleArea}");
Console.WriteLine($"Triangle area: {triangleArea}");
Console.WriteLine($"Circle area: {circleArea}");