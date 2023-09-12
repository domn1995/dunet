using BenchmarkDotNet.Attributes;
using Dunet.UnionAttributeGeneration;
using Dunet.UnionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Dunet.Benchmark;

[MemoryDiagnoser]
[InProcess]
public class SourceGeneratorBenchmarks
{
    const string sourceText =
        """
        using Dunet;

        [Union]
        public partial record Expression
        {
            partial record Number(int Value);

            partial record Add(Expression Left, Expression Right);

            partial record Multiply(Expression Left, Expression Right);

            partial record Variable(string Value);
        }

        [Union]
        partial record Shape
        {
            partial record Circle(double Radius);

            partial record Rectangle(double Length, double Width);

            partial record Triangle(double Base, double Height);
        }

        [Union]
        public partial record Option<T>
        {
            public static implicit operator Option<T>(T value) => new Some(value);

            partial record Some(T Value);

            partial record None();
        }
        """;
        
    private GeneratorDriver? _driver;
    private Compilation? _compilation;

    private (Compilation, CSharpGeneratorDriver) Setup(string source)
    {
        var compilation = CreateCompilation(source);
        if (compilation == null)
            throw new InvalidOperationException("Compilation returned null");

        var unionGenerator = new UnionGenerator();
        var unionAttributeGenerator = new UnionAttributeGenerator();

        var driver = CSharpGeneratorDriver.Create(unionGenerator, unionAttributeGenerator);
        
        return (compilation, driver);
    }

    [GlobalSetup(Target = nameof(Compile))]
    public void SetupCompile() => (_compilation, _driver) = Setup(sourceText);
    
    [GlobalSetup(Target = nameof(Cached))]
    public void SetupCached()
    {
        (_compilation, var driver) = Setup(sourceText);
        _driver = driver.RunGenerators(_compilation);
    }

    [Benchmark]
    public GeneratorDriver Compile() => _driver!.RunGeneratorsAndUpdateCompilation(_compilation!, out _, out _);
    
    [Benchmark]
    public GeneratorDriver Cached() => _driver!.RunGeneratorsAndUpdateCompilation(_compilation!, out _, out _);

    private static Compilation CreateCompilation(params string[] sources) =>
        CSharpCompilation.Create(
            "compilation",
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );
}