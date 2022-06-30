using System.Collections.Immutable;
using Dunet.UnionAttributeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dunet.Test.Compiler;

public class Compile
{
    private static readonly IIncrementalGenerator unionAttributeGenerator =
        new UnionAttributeGenerator();

    private readonly IIncrementalGenerator generator;

    public Compile(IIncrementalGenerator generator) => this.generator = generator;

    public static readonly ImmutableDictionary<string, string> DefaultGlobalConfig = ImmutableDictionary<string, string>.Empty
        .Add("build_property.Dunet_GenerateFactoryMethods", "false")
        .Add("build_property.Dunet_FactoryMethodPrefix", "New")
        .Add("build_property.Dunet_FactoryMethodSuffix", "");

    public CompilationResult ToAssembly(params string[] sources) =>
        ToAssembly(DefaultGlobalConfig, sources);
    
    public CompilationResult ToAssembly(ImmutableDictionary<string, string> globalOptions, params string[] sources)
    {
        var baseCompilation = CreateCompilation(sources);
        var (outputCompilation, compilationDiagnostics, generationDiagnostics) = RunGenerator(
            baseCompilation,
            globalOptions
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

    private GenerationResult RunGenerator(Compilation compilation, ImmutableDictionary<string, string> configOptions)
    {
        CSharpGeneratorDriver.Create(generator, unionAttributeGenerator)
            .WithUpdatedAnalyzerConfigOptions(new DictionaryAnalyzerConfigOptionsProvider(new DictionaryAnalyzerConfigOptions(configOptions)))
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
