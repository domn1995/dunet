# Dunet

**Dunet** is a simple source generator for [discriminated unions](https://en.wikipedia.org/wiki/Tagged_union) in C#.

## Install

- [NuGet](https://www.nuget.org/packages/Dunet/): `dotnet add package dunet`

## Usage

```cs
// 1. Import the namespace.
using Dunet;

// 2. Add the `Union` attribute to an interface.
[Union]
interface IShape
{
    // 3. Define the union members as interface methods.
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}

// 4. Use the union members.
IShape shape = new Rectangle(3, 4);
var area = shape switch
{
    Circle c => 3.14 * c.Radius * c.Radius,
    Rectangle r => r.Length * r.Width,
    Triangle t => t.Base * t.Height / 2,
    _ => 0d,
};

System.Console.WriteLine(area); // "12"
```

## Dedicated Match Method

Dunet will also generate a dedicated `Match()` extension method for the union type:

```cs
using Dunet;

[Union]
interface IChoice
{
    void Yes();
    void No(string Reason);
}

IChoice choice = new No("I don't wanna.");
var response = choice.Match(
    yes => "Yes!!!",
    no => $"No, because {no.reason}
);

System.Console.WriteLine(response); // "No, because I don't wanna."
```
