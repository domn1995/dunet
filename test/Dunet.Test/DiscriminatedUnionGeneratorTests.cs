using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Dunet.Test;

public class DiscriminatedUnionGeneratorTests
{
    [Fact]
    public void Compiles()
    {
        var compilation = CreateCompilation(@"
namespace Dunet.Cli;

[Union]
public interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}

public static class AreaCalculator
{
    public static double GetArea(IShape shape) => shape switch
    {
        Rectangle rect => rect.Length * rect.Width,
        Circle circle => 2.0 * System.Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };
}");
        var generator = new DiscriminatedUnionGenerator();
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var outputDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        Assert.Empty(outputDiagnostics);
    }

    private static Compilation CreateCompilation(string source) =>
        CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}