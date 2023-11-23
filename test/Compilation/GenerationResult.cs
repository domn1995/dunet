using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Dunet.Test.Compilation;

/// <summary>
/// Represents the result of running a C# source generator on a compilation.
/// </summary>
internal sealed record GenerationResult(
    Microsoft.CodeAnalysis.Compilation Compilation,
    ImmutableArray<Diagnostic> CompilationDiagnostics,
    ImmutableArray<Diagnostic> GenerationDiagnostics
)
{
    public ImmutableArray<Diagnostic> CompilationErrors =>
        CompilationDiagnostics
            .Where(static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error)
            .ToImmutableArray();

    public ImmutableArray<Diagnostic> GenerationErrors =>
        GenerationDiagnostics
            .Where(static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error)
            .ToImmutableArray();
}
