using System.Numerics;
using System.Globalization;

namespace StructPrototype;

public static class InputParser
{
    public static Option<Shape> ParseShape(string? input) =>
        input switch
        {
            "c" => Shape.OfCircle(1),
            "r" => Shape.OfRectangle(1, 1),
            "t" => Shape.OfTriangle(1, 1),
            _ => Option.OfNone<Shape>(),
        };

    public static Result<FormatException, T> ParseNumber<T>(string? input) where T : INumber<T> =>
        T.TryParse(input, CultureInfo.CurrentCulture, out var value)
            ? value
            : new FormatException($"Failed to parse '{input}' into type '{typeof(T).Name}'.");
}
