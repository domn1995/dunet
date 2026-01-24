using System.Collections.Immutable;
using System.Reflection;
using Dunet.Generator.UnionGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dunet.Test.Compilation;

/// <summary>
/// Enables compilation of C# source code with Dunet source generation.
/// </summary>
internal sealed class Compiler
{
    private static readonly IIncrementalGenerator unionGenerator = new UnionGenerator();
    private static readonly DiagnosticSuppressor unionSwitchExpressionDiagnosticSuppressor =
        new UnionSwitchExpressionDiagnosticSupressor();

    public static async Task<CompilationResult> CompileAsync(params string[] sources)
    {
        var compilationWithAnalyzers = CreateCompilation(sources);
        var generationResult = await RunGeneratorAsync(compilationWithAnalyzers);

        using var ms = new MemoryStream();
        Assembly? assembly = null;

        try
        {
            generationResult.Compilation.Emit(ms);
            assembly = Assembly.Load(ms.ToArray());
        }
        catch
        {
            // Do nothing since we want to inspect the diagnostics when compilation fails.
        }

        return new(Assembly: assembly, Diagnostics: generationResult.Diagnostics);
    }

    private static CompilationWithAnalyzers CreateCompilation(params string[] sources)
    {
        // Include metadata references for all currently loaded assemblies that have a physical location.
        // This keeps the test compilation environment in sync with the test host and avoids
        // manually listing specific runtime assemblies.
        var loadedAssemblyPaths = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => a.Location)
            // Include the Dunet assembly explicitly to ensure it's referenced.
            .Concat([typeof(UnionAttribute).Assembly.Location])
            .Distinct()
            .ToList();

        var references = loadedAssemblyPaths
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        var compilation = CSharpCompilation.Create(
            "compilation",
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            references,
            new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                reportSuppressedDiagnostics: true,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );

        return new CompilationWithAnalyzers(
            compilation,
            [unionSwitchExpressionDiagnosticSuppressor],
            new CompilationWithAnalyzersOptions(
                new AnalyzerOptions([]),
                onAnalyzerException: null,
                concurrentAnalysis: false,
                logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: true
            )
        );
    }

    private static async Task<GenerationResult> RunGeneratorAsync(
        CompilationWithAnalyzers compilationWithAnalyzers
    )
    {
        CSharpGeneratorDriver
            .Create(unionGenerator)
            .RunGeneratorsAndUpdateCompilation(
                compilationWithAnalyzers.Compilation,
                out var outputCompilation,
                out var generationDiagnostics
            );

        // Create a new CompilationWithAnalyzers with the generated compilation to apply suppressions
        var compilationWithAnalyzersAfterGeneration = new CompilationWithAnalyzers(
            outputCompilation,
            [unionSwitchExpressionDiagnosticSuppressor],
            new CompilationWithAnalyzersOptions(
                new AnalyzerOptions([]),
                onAnalyzerException: null,
                concurrentAnalysis: false,
                logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: true
            )
        );

        var diagnostics = await compilationWithAnalyzersAfterGeneration.GetAllDiagnosticsAsync();

        return new(Compilation: outputCompilation, Diagnostics: diagnostics);
    }
}
