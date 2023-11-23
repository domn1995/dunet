using System.Reflection;
using Dunet.UnionAttributeGeneration;
using Dunet.UnionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dunet.Test.Compilation;

/// <summary>
/// Enables compilation of C# source code with Dunet source generation.
/// </summary>
internal sealed class Compiler
{
    private static readonly IIncrementalGenerator unionAttributeGenerator =
        new UnionAttributeGenerator();

    private static readonly IIncrementalGenerator unionGenerator = new UnionGenerator();

    public static CompilationResult Compile(params string[] sources)
    {
        var baseCompilation = CreateCompilation(sources);
        var (outputCompilation, compilationDiagnostics, generationDiagnostics) = RunGenerator(
            baseCompilation
        );

        using var ms = new MemoryStream();
        Assembly? assembly = null;

        try
        {
            outputCompilation.Emit(ms);
            assembly = Assembly.Load(ms.ToArray());
        }
        catch
        {
            // Do nothing since we want to inspect the diagnostics when compilation fails.
        }

        return new(
            Assembly: assembly,
            CompilationDiagnostics: compilationDiagnostics,
            GenerationDiagnostics: generationDiagnostics
        );
    }

    private static Microsoft.CodeAnalysis.Compilation CreateCompilation(params string[] sources) =>
        CSharpCompilation.Create(
            "compilation",
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

    private static GenerationResult RunGenerator(Microsoft.CodeAnalysis.Compilation compilation)
    {
        CSharpGeneratorDriver
            .Create(unionGenerator, unionAttributeGenerator)
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
