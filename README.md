# Dunet

**Dunet** is a simple source generator for [discriminated unions](https://en.wikipedia.org/wiki/Tagged_union) in C#. 

## Usage

```cs 
// 1. Import the namespace.
using Dunet;

namespace Shapes;

// 2. Add the `Union` attribute to an interface.
[Union]
public interface IShape
{
	// 3. Define the union members as interface methods.
	IShape Circle(double radius);
	IShape Rectangle(double length, double width);
	IShape Triangle(double @base, double height);
}

// 4. Use the union members.
var shape = new Rectangle(3, 4);
var area = shape switch
{
	Circle c => 3.14 * c.Radius * c.Radius,
	Rectangle r => c.Length * c.Width,
	Triangle t => t.Base * t.Height / 2,
	_ => 0d,
};
```

## Install

- [NuGet](https://placeholder.com): `dotnet add package dunet`

