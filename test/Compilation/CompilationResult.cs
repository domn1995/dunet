using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Dunet.Test.Compilation;

/// <summary>
/// Represents the result of compiling C# source code.
/// </summary>
internal sealed record CompilationResult(Assembly? Assembly, ImmutableArray<Diagnostic> Diagnostics)
{
    public ImmutableArray<Diagnostic> Warnings =>
        Diagnostics
            .Where(static diagnostic =>
                diagnostic.Severity is DiagnosticSeverity.Warning && !diagnostic.IsSuppressed
            )
            .ToImmutableArray();

    public ImmutableArray<Diagnostic> Errors =>
        Diagnostics
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error)
            .ToImmutableArray();
}
