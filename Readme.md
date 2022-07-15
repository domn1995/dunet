# Dunet

**Dunet** is a simple source generator for [discriminated unions](https://en.wikipedia.org/wiki/Tagged_union) in C#.

## Install

- [NuGet](https://www.nuget.org/packages/Dunet/): `dotnet add package dunet`

## Usage

```cs
// 1. Import the namespace.
using Dunet;

// 2. Add the `Union` attribute to a partial record.
[Union]
partial record Shape
{
    // 3. Define the union members as inner partial records.
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}

// 4. Use the union members.
var shape = new Shape.Rectangle(3, 4);
var area = shape.Match(
    circle => 3.14 * circle.Radius * circle.Radius,
    rectangle => rectangle.Length * rectangle.Width,
    triangle => triangle.Base * triangle.Height / 2
);
Console.WriteLine(area); // "12"
```

## Generics Support

Use generics for more advanced union types. For example, an option monad:

```cs
// 1. Import the namespace.
using Dunet;
// Optional: use aliasing for more terse code.
using static Option<int>;

// 2. Add the `Union` attribute to a partial record.
// 3. Add one or more type arguments to the union record.
[Union]
partial record Option<T>
{
    partial record Some(T Value);
    partial record None();
}

// 4. Use the union members.
Option<int> ParseInt(string? value) =>
    int.TryParse(value, out var number)
        ? new Some(number)
        : new None();

string GetOutput(Option<int> number) =>
    number.Match(
        some => some.Value.ToString(),
        none => "Invalid input!"
    );

var input = Console.ReadLine(); // User inputs "not a number".
var result = ParseInt(input);
var output = GetOutput(result);
Console.WriteLine(output); // "Invalid input!"

input = Console.ReadLine(); // User inputs "12345".
result = ParseInt(input);
output = GetOutput(result);
Console.WriteLine(output); // "12345".
```

## Implicit Conversion Support

Given a union type where all members contain only a single parameter
and all parameters are a different type, Dunet will generate implicit
conversions between their values and the union type.

For example, consider a `Result` union type that represents success
as a `double` and failure as an `Exception`:

```cs
// 1. Import the namespace.
using Dunet;

// 2. Define a union type with single unique member values:
[Union]
partial record Result
{
    partial record Success(double Value);
    partial record Failure(Exception Error);
}

// 3. Return union member values directly.
Result Divide(double numerator, double denominator)
{
    if (denominator is 0d)
    {
        // No need for `new Result.Failure(new InvalidOperationException("..."));`
        return new InvalidOperationException("Cannot divide by zero!");
    }

    // No need for `new Result.Success(...);`
    return numerator / denominator;
}

var result = Divide(42, 0);
var output = result.Match(
    success => success.Value.ToString(),
    failure => failure.Error.Message
);

Console.WriteLine(output); // "Cannot divide by zero!"
```

## Samples

- [Area Calculator](./samples/AreaCalculator/Program.cs)
- [Serialization/Deserialization](./samples/Serialization/Program.cs)
- [Option Monad](./samples/OptionMonad/Program.cs)
- [Web Client](./samples/PokemonClient/PokeClient.cs)
- [Recursive Expressions](./samples/ExpressionCalculator/Program.cs)
