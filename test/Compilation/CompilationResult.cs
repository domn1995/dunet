using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;

namespace Dunet.Test.Compilation;

/// <summary>
/// Represents the result of compiling C# source code.
/// </summary>
internal sealed record CompilationResult(
    Assembly? Assembly,
    ImmutableArray<Diagnostic> CompilationDiagnostics,
    ImmutableArray<Diagnostic> GenerationDiagnostics
)
{
    public ImmutableArray<Diagnostic> CompilationErrors =>
        CompilationDiagnostics
            .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error)
            .ToImmutableArray();

    public ImmutableArray<Diagnostic> GenerationErrors =>
        GenerationDiagnostics
            .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error)
            .ToImmutableArray();
}
