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

// 2. Add the `Union` attribute to a partial record.
// 3. Add one or more type arguments to the union record.
[Union]
partial record Option<T>
{
    partial record Some(T Value);
    partial record None();
}

// 4. Use the union members.
static Option<int> ParseInt(string? value) =>
    int.TryParse(value, out var number)
        ? Option<int>.Some(number)
        : Option<int>.None();

static string GetOutput(Option<int> number) =>
    number.Match(
        some => some.Value.ToString(),
        none => "Invalid input!"
    );

var input = ParseInt(Console.ReadLine()); // User inputs "not a number".
Console.WriteLine(GetOutput(input)); // "Invalid input!"

input = ParseInt(Console.ReadLine()); // User inputs "12345".
Console.WriteLine(GetOutput(input)); // "12345".
```

## Samples

- [Area Calculator](./samples/AreaCalculator/Program.cs)
- [Serialization/Deserialization](./samples/Serialization/Program.cs)
- [Option Monad](./samples/OptionMonad/Program.cs)
- [Web Client](./samples/PokemonClient/PokeClient.cs)
