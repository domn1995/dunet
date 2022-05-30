using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Dunet.Test;

public class DiscriminatedUnionGeneratorTests
{
    [Fact]
    public void Runs()
    {
        var compilation = CreateCompilation(@"
namespace DuTests.Runs;

public class DiscriminatedUnion
{
    public static void Run()
    {
    }
}");
        var generator = new DiscriminatedUnionGenerator();
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var outputDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        Assert.Empty(outputDiagnostics);
    }

    private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}