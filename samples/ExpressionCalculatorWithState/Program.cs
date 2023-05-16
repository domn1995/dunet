using Dunet;
using static Expression;

var environment = new Dictionary<string, int>()
{
    ["a"] = 1,
    ["b"] = 2,
    ["c"] = 3,
};

var expression = new Add(new Variable("a"), new Multiply(new Number(2), new Variable("b")));
var result = Evaluate(environment, expression);

Console.WriteLine(result); // "5"

static int Evaluate(Dictionary<string, int> env, Expression exp) =>
    exp.Match(
        // 1. Pass your state "container" as the first parameter.
        state: env,
        // 2. Use static lambdas for each variant's match method.
        static (_, number) => number.Value,
        // 3. Reference the state as the first argument of each lambda.
        static (state, add) => Evaluate(state, add.Left) + Evaluate(state, add.Right),
        static (state, mul) => Evaluate(state, mul.Left) * Evaluate(state, mul.Right),
        static (state, var) => state[var.Value]
    );

[Union]
public partial record Expression
{
    public partial record Number(int Value);

    public partial record Add(Expression Left, Expression Right);

    public partial record Multiply(Expression Left, Expression Right);

    public partial record Variable(string Value);
}
