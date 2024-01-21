using System.Text.Json;
using System.Text.Json.Serialization;
using Dunet;
using static Shape;

var circle = new Circle(10);
var rectangle = new Rectangle(10, 10);
var triangle = new Triangle(10, 10);

var shapes = new Shape[] { circle, rectangle, triangle };

var shapesJson = JsonSerializer.Serialize(shapes);
Console.WriteLine(shapesJson);

// NOTE: The type discriminator must be the first property.
var deserializedShapes = JsonSerializer.Deserialize<Shape[]>(
    //lang=json
    """
    [
        {
            "$type": "Circle",
            "radius": 10
        },
        {
            "$type": "Rectangle",
            "length": 10,
            "width": 10
        },
        {
            "$type": "Triangle",
            "base": 10,
            "height": 10
        }
    ]
    """,
    // So we recognize camel case properties.
    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }
);

foreach (var shape in deserializedShapes!)
{
    Console.WriteLine(shape);
}

// Serialization/deserialization can be enabled with `JsonDerivedType` attribute from .NET 7.
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
