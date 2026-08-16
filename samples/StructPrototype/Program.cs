using StructPrototype;

while (true)
{
    Console.Write("Enter a shape (c, r, t): ");
    var shapeInput = Console.ReadLine();
    var shape = InputParser.ParseShape(shapeInput);
    var area = shape switch
    {
        Option<Shape>.Some some => Calculator.Area(some.Value),
        Option<Shape>.None => 0
    };
    Console.WriteLine($"Area = {area}");

    Console.WriteLine("Enter a numerator: ");
    var numeratorInput = InputParser.ParseNumber<double>(Console.ReadLine());
    Console.WriteLine("Enter a denominator: ");
    var denominatorInput = InputParser.ParseNumber<int>(Console.ReadLine());

    // When there's no flatmap... 😅
    var result = numeratorInput switch
    {
        FormatException err => err.Message,
        double numerator => denominatorInput switch
        {
            FormatException err => err.Message,
            int denominator => Calculator.Divide(numerator, denominator) switch
            {
                ArithmeticException err => err.Message,
                double ok => ok.ToString(),
            }
        }
    };

    Console.WriteLine($"Result = {result}");
}
