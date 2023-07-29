using StructPrototype;

while (true)
{
    Console.Write("Enter a shape (c, r, t): ");
    var shapeInput = Console.ReadLine();
    var shape = InputParser.ParseShape(shapeInput);
    var area = shape.Match(some => Calculator.Area(some.Value), (none) => 0);
    Console.WriteLine($"Area = {area}");

    Console.WriteLine("Enter a numerator: ");
    var numeratorInput = Console.ReadLine();
    Console.WriteLine("Enter a denominator: ");
    var denominatorInput = Console.ReadLine();
    var numerator = InputParser.ParseNumber<double>(numeratorInput);
    var denominator = InputParser.ParseNumber<int>(denominatorInput);

    // When there's no flatmap... 😅
    var divisionResult = numerator.Match(
        err => err.Error.Message,
        top =>
            denominator.Match(
                err => err.Error.Message,
                bottom =>
                    Calculator
                        .Divide(top.Value, bottom.Value)
                        .Match(
                            divisionErr => $"{divisionErr.Error}",
                            divisionResult => divisionResult.Value.ToString()
                        )
            )
    );

    Console.WriteLine(divisionResult);
}
