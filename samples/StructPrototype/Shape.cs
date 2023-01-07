using System.Diagnostics;

namespace StructPrototype;

public partial record struct Shape
{
    public record struct Circle(double Radius);

    public record struct Rectangle(double Height, double Width);

    public record struct Triangle(double Base, double Height);
}

public partial record struct Shape
{
    private enum ShapeType : byte
    {
        Circle,
        Rectangle,
        Triangle,
    };

    private ShapeType type;

    private Circle circle;
    private Rectangle rectangle;
    private Triangle triangle;

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

    public static class Prelude
    {
        public static Shape Circle(double radius) =>
            new() { circle = new Circle(radius), type = ShapeType.Circle, };

        public static Shape Rectangle(double height, double width) =>
            new() { rectangle = new Rectangle(height, width), type = ShapeType.Rectangle, };

        public static Shape Triangle(double @base, double height) =>
            new() { triangle = new Triangle(@base, height), type = ShapeType.Triangle, };
    }
}

public static class ShapePrelude
{
    public static Shape Circle(double radius) => Shape.Prelude.Circle(radius);

    public static Shape Rectangle(double height, double width) =>
        Shape.Prelude.Rectangle(height, width);

    public static Shape Triangle(double @base, double height) =>
        Shape.Prelude.Triangle(@base, height);
}
