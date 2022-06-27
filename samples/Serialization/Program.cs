using Dunet;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Shape;

var circle = new Circle(10);
var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);

var shapes = new Shape[] { circle, rectangle, triangle };

var shapesJson = JsonSerializer.Serialize(shapes);
var deserializedShapes = JsonSerializer.Deserialize<Shape[]>(shapesJson);

Console.WriteLine(shapesJson);

foreach (var shape in deserializedShapes!)
{
    Console.WriteLine(shape);
}

// Serialization/deserialization can be eanbled with `JsonDerivedType` attribute from .NET 7.
[Union]
[JsonDerivedType(typeof(Circle), typeDiscriminator: nameof(Circle))]
[JsonDerivedType(typeof(Rectangle), typeDiscriminator: nameof(Rectangle))]
[JsonDerivedType(typeof(Triangle), typeDiscriminator: nameof(Triangle))]
partial record Shape
{
    partial record Circle(double Radius);

    partial record Rectangle(double Length, double Width);

    partial record Triangle(double Base, double Height);
}
