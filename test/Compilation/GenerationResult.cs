using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Dunet.Test.Compilation;

internal sealed record GenerationResult(
    Microsoft.CodeAnalysis.Compilation Compilation,
    ImmutableArray<Diagnostic> Diagnostics
);
