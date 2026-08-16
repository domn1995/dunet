using System.Numerics;
using System.Globalization;

namespace StructPrototype;

public static class InputParser
{
    public static Option<Shape> ParseShape(string? input) =>
        input switch
        {
            "c" => Option.Some<Shape>(new Shape.Circle(1)),
            "r" => Option.Some<Shape>(new Shape.Rectangle(1, 1)),
            "t" => Option.Some<Shape>(new Shape.Triangle(1, 1)),
            _ => Option.None<Shape>(),
        };

    public static Result<FormatException, T> ParseNumber<T>(string? input) where T : INumber<T> =>
        T.TryParse(input, CultureInfo.CurrentCulture, out var value)
            ? value
            : new FormatException($"Failed to parse '{input}' into type '{typeof(T).Name}'.");
}
