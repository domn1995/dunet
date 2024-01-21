using Dunet;
using static Option<int>;

while (true)
{
    Console.Write("Please enter a number: ");
    var input = Console.ReadLine();
    var result = ParseInt(input);

    var output = result.Match(
        static some => $"You entered the number: {some.Value}",
        static none => "That's not a number!"
    );

    Console.WriteLine(output);
}

static Option<int> ParseInt(string? value) => int.TryParse(value, out var num) ? num : new None();

[Union]
public partial record Option<T>
{
    public static implicit operator Option<T>(T value) => new Some(value);

    partial record Some(T Value);

    partial record None();
}
