using StructPrototype;

while (true)
{
    Console.Write("Enter a shape (c, r, t): ");
    var shapeInput = Console.ReadLine();
    var shape = InputParser.ParseShape(shapeInput);
    var area = shape.Match(Calculator.Area, () => 0);
    Console.WriteLine($"Area = {area}");

    Console.WriteLine("Enter a numerator: ");
    var numeratorInput = Console.ReadLine();
    Console.WriteLine("Enter a denominator: ");
    var denominatorInput = Console.ReadLine();
    var numerator = InputParser.ParseNumber<double>(numeratorInput);
    var denominator = InputParser.ParseNumber<double>(denominatorInput);

    // When you don't implement `SelectMany()` 😅
    var result = numerator.Match(
        err: numErr => numErr,
        ok: top =>
            denominator.Match(
                err: denErr => denErr,
                ok: bottom =>
                    Calculator
                        .Divide(top, bottom)
                        .Match<Result<string, double>>(
                            err: divErr => divErr.Message,
                            ok: divOk => divOk
                        )
            )
    );

    var output = result.Match(err => $"Error: {err}", ok => ok.ToString());

    Console.WriteLine(output);
}
