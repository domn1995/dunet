namespace StructPrototype;

using System.Numerics;

public static class Calculator
{
    public static double Area(Shape shape) =>
        shape.Match(
            circle => 3.14 * circle.Radius,
            rectangle => rectangle.Height * rectangle.Width,
            triangle => 0.5 * triangle.Base * triangle.Height
        );

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
