using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Dunet.Test.Compiler;

public class Compile
{
    private static readonly DiscriminatedUnionGenerator generator = new();

    public static CompilationResult ToAssembly(params string[] sources)
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

    private static GenerationResult RunGenerator(Compilation compilation)
    {
        CSharpGeneratorDriver
            .Create(generator)
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
