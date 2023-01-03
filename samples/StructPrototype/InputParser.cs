using static StructPrototype.ShapePrelude;
using static StructPrototype.OptionPrelude;
using static StructPrototype.ResultPrelude;
using System.Numerics;

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

    public static Result<string, T> ParseNumber<T>(string? input) where T : INumber<T> =>
        T.TryParse(input, null, out var value)
            ? Ok<string, T>(value)
            : Err<string, T>("Invalid number");
}
