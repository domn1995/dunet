// Adapted from https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions#using-discriminated-unions-for-tree-data-structures

using Dunet;
using static Expression;

var environment = new Dictionary<string, int>()
{
    ["a"] = 1,
    ["b"] = 2,
    ["c"] = 3,
};

var expression = new Add(new Variable("a"), new Multiply(new Number(2), new Variable("b")));

// Evaluate a + 2 * b
var result = Evaluate(environment, expression);

Console.WriteLine(result); // 5

static int Evaluate(Dictionary<string, int> env, Expression exp) =>
    exp switch
    {
        Number(var value) => value,
        Add(var left, var right) => Evaluate(env, left) + Evaluate(env, right),
        Multiply(var left, var right) => Evaluate(env, left) * Evaluate(env, right),
        Variable(var value) => env[value],
    };

[Union]
public partial record Expression
{
    partial record Number(int Value);

    partial record Add(Expression Left, Expression Right);

    partial record Multiply(Expression Left, Expression Right);

    partial record Variable(string Value);
}
