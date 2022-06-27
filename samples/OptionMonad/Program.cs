using Dunet;
using static Option<int>;

while (true)
{
    Console.Write("Please enter a number: ");
    var input = ParseInt(Console.ReadLine());

    var output = input.Match(
        some => $"You entered the number: {some.Value}",
        none => "That's not a number!"
    );

    Console.WriteLine(output);
}

static Option<int> ParseInt(string? value) =>
    int.TryParse(value, out var num) ? new Some(num) : new None();

[Union]
public partial record Option<T>
{
    partial record Some(T Value);

    partial record None();
}
