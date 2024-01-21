using System.Reflection;
using Dunet.Generator.UnionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dunet.Test.Compilation;

/// <summary>
/// Enables compilation of C# source code with Dunet source generation.
/// </summary>
internal sealed class Compiler
{
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

    private static CSharpCompilation CreateCompilation(params string[] sources) =>
        CSharpCompilation.Create(
            "compilation",
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            [
                // Resolves to System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                // Resolves to System.Runtime.dll, which is needed for the Attribute type
                // Can't use typeof(Attribute).GetTypeInfo().Assembly.Location because it resolves to System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(
                    AppDomain
                        .CurrentDomain.GetAssemblies()
                        .First(static assembly =>
                            assembly.FullName?.Contains("System.Runtime") is true
                        )
                        .Location
                ),
                MetadataReference.CreateFromFile(
                    typeof(UnionAttribute).GetTypeInfo().Assembly.Location
                )
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

    private static GenerationResult RunGenerator(Microsoft.CodeAnalysis.Compilation compilation)
    {
        CSharpGeneratorDriver
            .Create(unionGenerator)
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
