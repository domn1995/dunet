using System.Runtime.CompilerServices;

namespace StructPrototype;


public partial record struct Shape
{
    public record struct Circle(double Radius);

    public record struct Rectangle(double Height, double Width);

    public record struct Triangle(double Base, double Height);
}

[System.Runtime.CompilerServices.Union]
public readonly partial record struct Shape : IUnion
{
    private enum ShapeType : byte
    {
        Circle,
        Rectangle,
        Triangle,
    };

    private readonly ShapeType? type;

    private readonly Circle? circle;
    private readonly Rectangle? rectangle;
    private readonly Triangle? triangle;

    public Shape(Circle circle)
    {
        this.type = ShapeType.Circle;
        this.circle = circle;
    }

    public Shape(Rectangle rectangle)
    {
        this.type = ShapeType.Rectangle;
        this.rectangle = rectangle;
    }

    public Shape(Triangle triangle)
    {
        this.type = ShapeType.Triangle;
        this.triangle = triangle;
    }

    public bool HasValue => type is not null;

    public object? Value => type switch
    {
        ShapeType.Circle => circle,
        ShapeType.Rectangle => rectangle,
        ShapeType.Triangle => triangle,
        _ => null,
    };

    public bool TryGetValue(out Circle? value)
    {
        value = this.circle;
        return type is ShapeType.Circle;
    }

    public bool TryGetValue(out Rectangle? value)
    {
        value = this.rectangle;
        return type is ShapeType.Rectangle;
    }

    public bool TryGetValue(out Triangle? value)
    {
        value = this.triangle;
        return type is ShapeType.Triangle;
    }
}
