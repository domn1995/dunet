namespace StructPrototype;

using System.Numerics;

public static class Calculator
{
    public static double Area(Shape shape) =>
        shape switch
        {
            Shape.Circle circle => 3.14 * circle.Radius,
            Shape.Rectangle rectangle => rectangle.Height * rectangle.Width,
            Shape.Triangle triangle => 0.5 * triangle.Base * triangle.Height
        };

    public static Result<ArithmeticException, T> Divide<T>(T numerator, T denominator)
        where T : INumber<T>
    {
        if (denominator == T.Zero)
        {
            return new ArithmeticException("Division by zero");
        }

        return numerator / denominator;
    }
}
