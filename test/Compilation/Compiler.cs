using System;
using System.Collections.Generic;
using System.Linq;
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

    private static CSharpCompilation CreateCompilation(params string[] sources)
    {
        // Include metadata references for all currently loaded assemblies that have a physical location.
        // This keeps the test compilation environment in sync with the test host and avoids
        // manually listing specific runtime assemblies.
        var loadedAssemblyPaths = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => a.Location)
            .Distinct()
            .ToList();

        var references = loadedAssemblyPaths
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        // Ensure the assembly containing UnionAttribute is referenced in case it's not already loaded.
        var unionAssemblyLocation = typeof(UnionAttribute).GetTypeInfo().Assembly.Location;
        if (
            !string.IsNullOrEmpty(unionAssemblyLocation)
            && !loadedAssemblyPaths.Contains(unionAssemblyLocation)
        )
        {
            references.Add(MetadataReference.CreateFromFile(unionAssemblyLocation));
        }

        return CSharpCompilation.Create(
            "compilation",
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );
    }

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
