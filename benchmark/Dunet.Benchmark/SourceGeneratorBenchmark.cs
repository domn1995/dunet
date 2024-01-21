using System.Reflection;
using BenchmarkDotNet.Attributes;
using Dunet.Generator.UnionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dunet.Benchmark;

[MemoryDiagnoser]
[InProcess]
public class SourceGeneratorBenchmarks
{
    const string sourceText = """
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

    /// Guaranteed to be initialized by the benchmark global setup.
    private GeneratorDriver driver = null!;
    private Compilation compilation = null!;

    private static (Compilation, CSharpGeneratorDriver) SetUp(string source)
    {
        var compilation =
            CreateCompilation(source)
            ?? throw new InvalidOperationException("Compilation returned null");

        var unionGenerator = new UnionGenerator();

        var driver = CSharpGeneratorDriver.Create(unionGenerator);

        return (compilation, driver);
    }

    [GlobalSetup(Target = nameof(Compile))]
    public void SetUpCompile() => (compilation, driver) = SetUp(sourceText);

    [GlobalSetup(Target = nameof(Cached))]
    public void SetUpCached()
    {
        (compilation, var driver) = SetUp(sourceText);
        this.driver = driver.RunGenerators(compilation);
    }

    [Benchmark]
    public GeneratorDriver Compile() =>
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

    [Benchmark]
    public GeneratorDriver Cached() =>
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

    private static CSharpCompilation CreateCompilation(params string[] sources) =>
        CSharpCompilation.Create(
            assemblyName: "compilation",
            syntaxTrees: sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            references:
            [
                // Resolves to System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                // Resolves to System.Runtime.dll, which is needed for the Attribute type
                // Can't use typeof(Attribute).GetTypeInfo().Assembly.Location because it resolves to System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(
                    AppDomain
                        .CurrentDomain.GetAssemblies()
                        .First(f => f.FullName?.Contains("System.Runtime") is true)
                        .Location
                ),
                MetadataReference.CreateFromFile(
                    typeof(UnionAttribute).GetTypeInfo().Assembly.Location
                )
            ],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );
}
