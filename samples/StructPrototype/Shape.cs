using System.Diagnostics;

namespace StructPrototype;

public partial record struct Shape
{
    public partial record struct Circle(double Radius);

    public partial record struct Rectangle(double Height, double Width);

    public partial record struct Triangle(double Base, double Height);
}

public readonly partial record struct Shape
{
    private enum ShapeType
    {
        Circle,
        Rectangle,
        Triangle,
    };

    private Shape(Circle @circle)
    {
        this.@circle = @circle;
        this.type = ShapeType.Circle;
    }

    private Shape(Rectangle @rectangle)
    {
        this.@rectangle = @rectangle;
        type = ShapeType.Rectangle;
    }

    private Shape(Triangle @triangle)
    {
        this.@triangle = @triangle;
        type = ShapeType.Triangle;
    }

    private readonly ShapeType type;
    private readonly Circle @circle;
    private readonly Rectangle @rectangle;
    private readonly Triangle @triangle;

    public TOut Match<TOut>(
        Func<Circle, TOut> @circle,
        Func<Rectangle, TOut> @rectangle,
        Func<Triangle, TOut> @triangle
    ) =>
        type switch
        {
            ShapeType.Circle => @circle(this.@circle),
            ShapeType.Rectangle => @rectangle(this.@rectangle),
            ShapeType.Triangle => @triangle(this.@triangle),
            var invalid
                => throw new UnreachableException(
                    $"Matched an unreachable union variant: {invalid}"
                ),
        };

    public static Shape NewCircle(double @radius) => new(new Circle(@radius));

    public static Shape NewRectangle(double @height, double @width) =>
        new(new Rectangle(@height, @width));

    public static Shape NewTriangle(double @base, double @height) =>
        new(new Triangle(@base, @height));
}
