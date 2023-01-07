using static StructPrototype.ShapePrelude;
using static StructPrototype.OptionPrelude;
using System.Numerics;
using System.Globalization;

namespace StructPrototype;

public static class InputParser
{
    public static Option<Shape> ParseShape(string? input) =>
        input switch
        {
            "c" => Circle(1),
            "r" => Rectangle(1, 1),
            "t" => Triangle(1, 1),
            _ => None<Shape>(),
        };

    public static Result<FormatException, T> ParseNumber<T>(string? input) where T : INumber<T> =>
        T.TryParse(input, CultureInfo.CurrentCulture, out var value)
            ? value
            : new FormatException($"Failed to parse '{input}' into type '{typeof(T).Name}'.");
}
