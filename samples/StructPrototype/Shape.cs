using System.Diagnostics;

namespace StructPrototype;

public record struct Shape
{
    public record struct Circle(double Radius);

    public record struct Rectangle(double Height, double Width);

    public record struct Triangle(double Base, double Height);

    private enum ShapeType : byte
    {
        Circle,
        Rectangle,
        Triangle,
    };

    private ShapeType type;

    private Circle circle;

    public static Shape NewCircle(double radius) =>
        new() { circle = new Circle(radius), type = ShapeType.Circle, };

    public Circle UnwrapCircle() =>
        type switch
        {
            ShapeType.Circle => circle,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {ShapeType.Circle} but got {actual}"
                ),
        };

    private Rectangle rectangle;

    public static Shape NewRectangle(double height, double width) =>
        new() { rectangle = new Rectangle(height, width), type = ShapeType.Rectangle, };

    public Rectangle UnwrapRectangle() =>
        type switch
        {
            ShapeType.Rectangle => rectangle,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {ShapeType.Rectangle} but got {actual}"
                ),
        };

    private Triangle triangle;

    public static Shape NewTriangle(double @base, double height) =>
        new() { triangle = new Triangle(@base, height), type = ShapeType.Triangle, };

    public Triangle UnwrapTriangle() =>
        type switch
        {
            ShapeType.Triangle => triangle,
            var actual
                => throw new InvalidOperationException(
                    $"Expected {ShapeType.Triangle} but got {actual}"
                ),
        };

    public TOut Match<TOut>(
        Func<Circle, TOut> circle,
        Func<Rectangle, TOut> rectangle,
        Func<Triangle, TOut> triangle
    ) =>
        type switch
        {
            ShapeType.Circle => circle(this.circle),
            ShapeType.Rectangle => rectangle(this.rectangle),
            ShapeType.Triangle => triangle(this.triangle),
            var invalid
                => throw new UnreachableException($"Matched an unreachable union type: {invalid}"),
        };
}

public static class ShapePrelude
{
    public static Shape Circle(double radius) => Shape.NewCircle(radius);

    public static Shape Rectangle(double height, double width) => Shape.NewRectangle(height, width);

    public static Shape Triangle(double @base, double height) => Shape.NewTriangle(@base, height);
}
