# Dunet

[![Build](https://img.shields.io/github/actions/workflow/status/domn1995/dunet/main.yml?branch=main)](https://github.com/domn1995/dunet/actions)
[![Package](https://img.shields.io/nuget/v/dunet.svg)](https://nuget.org/packages/dunet)

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
```

```cs
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
// Optional: use static import for more terse code.
using static Option<int>;

// 2. Add the `Union` attribute to a partial record.
// 3. Add one or more type arguments to the union record.
[Union]
partial record Option<T>
{
    partial record Some(T Value);
    partial record None();
}
```

```cs
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

Dunet generates implicit conversions between union member values and the union type if your union meets all of the following conditions:

- All members contain only a single parameter.
- All parameters are a different type.
- No parameters are an interface type.

For example, consider a `Result` union type that represents success as a `double` and failure as an `Exception`:

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
```

```cs
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

## Async Match Support

Dunet generates a `MatchAsync()` extension method for all `Task<T>` and `ValueTask<T>` where `T` is a union type. For example:

```cs
// Choice.cs

using Dunet;

namespace Core;

// 1. Define a union type within a namespace.
[Union]
partial record Choice
{
    partial record Yes;
    partial record No(string Reason);
}
```

```cs
// Program.cs

using Core;
using static Core.Choice;

// 2. Define async methods like you would for any other type.
static async Task<Choice> AskAsync()
{
    // Simulating network call.
    await Task.Delay(1000);

    // 3. Return unions from async methods like any other type.
    return new No("because I don't wanna!");
}

// 4. Asynchronously match any union `Task` or `ValueTask`.
var response = await AskAsync()
    .MatchAsync(
        yes => "Yes!!!",
        no => $"No, {no.Reason}"
    );

// Prints "No, because I don't wanna!" after 1 second.
Console.WriteLine(response);
```

> **Note**:
> `MatchAsync()` can only be generated for namespaced unions.

## Match on Specific Union Member

Dunet generates specific match methods for each union member. This is useful when unwrapping a union and you only care about transforming a single variant. For example:

```cs
[Union]
partial record Shape
{
    partial record Point(int X, int Y);
    partial record Line(double Length);
    partial record Rectangle(double Length, double Width);
    partial record Sphere(double Radius);
}
```

```cs
public static bool IsZeroDimensional(this Shape shape) =>
    shape.MatchPoint(
        point => true,
        () => false
    );

public static bool IsOneDimensional(this Shape shape) =>
    shape.MatchLine(
        line => true,
        () => false
    );

public static bool IsTwoDimensional(this Shape shape) =>
    shape.MatchRectangle(
        rectangle => true,
        () => false
    );

public static bool IsThreeDimensional(this Shape shape) =>
    shape.MatchSphere(
        sphere => true,
        () => false
    );
```

## Pretty Print Union Variants

To control how union variants are printed with their `ToString()` methods, override and seal the union declaration's `ToString()` method. For example:

```cs
[Union]
public partial record QueryResult<T>
{
    public partial record Ok(T Value);
    public partial record NotFound;
    public partial record Unauthorized;

    public sealed override string ToString() =>
        Match(
            ok => ok.Value.ToString(),
            entityNotFound => "Not found.",
            unauthorizedAccess => "Unauthorized access."
        );
}
```

> **Note**:
> You must seal the `ToString()` override to prevent the compiler from synthesizing a custom `ToString()` method for each variant.
>
> More info: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display

## Nested Union Support

To declare a union nested within a class or record, the class or record must be `partial`. For example:

```cs
// This type declaration must be partial.
public partial class Parent1
{
    // So must this one.
    public partial class Parent2
    {
        // Unions must always be partial.
        [Union]
        public partial record Nested
        {
            public partial record Member1;
            public partial record Member2;
        }
    }
}
```

```cs
// Access union members like any other nested type.
var member1 = new Parent1.Parent2.Nested.Member1();
```

## Samples

- [Area Calculator](./samples/AreaCalculator/Program.cs)
- [Serialization/Deserialization](./samples/Serialization/Program.cs)
- [Option Monad](./samples/OptionMonad/Program.cs)
- [Web Client](./samples/PokemonClient/PokeClient.cs)
- [Recursive Expressions](./samples/ExpressionCalculator/Program.cs)
