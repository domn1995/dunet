using System.Numerics;
using System.Globalization;

namespace StructPrototype;

public static class InputParser
{
    public static Option<Shape> ParseShape(string? input) =>
        input switch
        {
            "c" => Shape.NewCircle(1),
            "r" => Shape.NewRectangle(1, 1),
            "t" => Shape.NewTriangle(1, 1),
            _ => Option<Shape>.NewNone(),
        };

    public static Result<FormatException, T> ParseNumber<T>(string? input) where T : INumber<T> =>
        T.TryParse(input, CultureInfo.CurrentCulture, out var value)
            ? value
            : new FormatException($"Failed to parse '{input}' into type '{typeof(T).Name}'.");
}
