using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Dunet.Test;

public class DiscriminatedUnionGeneratorTests
{
    [Fact]
    public void Compiles()
    {
        // Arrange.
        var source = @"
namespace Dunet.Cli;

[Union]
public interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}";
        // Act.
        var (output, diagnostics) = RunGenerator(source);
        var outputDiagnostics = output?.GetDiagnostics();

        // Assert.
        Assert.Empty(diagnostics);
        Assert.Empty(outputDiagnostics);
    }

    private static Compilation Compile(string source) =>
        CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static (Compilation? output, ImmutableArray<Diagnostic> diagnostics) RunGenerator(string source)
    {
        var compilation = Compile(source);
        var generator = new DiscriminatedUnionGenerator();
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        return (outputCompilation, diagnostics);
    }
}