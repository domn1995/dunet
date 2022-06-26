using Dunet.UnionAttributeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace Dunet.Test.Compiler;

public class Compile
{
    private static readonly IIncrementalGenerator unionAttributeGenerator =
        new UnionAttributeGenerator();

    private readonly IIncrementalGenerator generator;

    public Compile(IIncrementalGenerator generator) => this.generator = generator;

    public CompilationResult ToAssembly(params string[] sources)
    {
        var baseCompilation = CreateCompilation(sources);
        var (outputCompilation, compilationDiagnostics, generationDiagnostics) = RunGenerator(
            baseCompilation
        );
        using var ms = new MemoryStream();
        outputCompilation.Emit(ms);
        var assembly = Assembly.Load(ms.ToArray());
        return new(
            Assembly: assembly,
            CompilationDiagnostics: compilationDiagnostics,
            GenerationDiagnostics: generationDiagnostics
        );
    }

    private static Compilation CreateCompilation(params string[] sources) =>
        CSharpCompilation.Create(
            "compilation",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)),
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

    private GenerationResult RunGenerator(Compilation compilation)
    {
        CSharpGeneratorDriver
            .Create(generator, unionAttributeGenerator)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var generationDiagnostics
            );

        return new(
            Compilation: outputCompilation,
            CompilationDiagnostics: outputCompilation.GetDiagnostics(),
            GenerationDiagnostics: generationDiagnostics
        );
    }
}
