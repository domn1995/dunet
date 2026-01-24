namespace Dunet.Test.Compilation;

internal static class DiagnosticAssertions
{
    private static readonly string switchExpressionDiagnosticId = "CS8509";

    extension(CompilationResult self)
    {
        public void ShouldNotContainSwitchExpressionWarning()
        {
            self.Warnings.Should()
                .NotContain(
                    static diagnostic => diagnostic.Id == switchExpressionDiagnosticId,
                    because: "it should be suppressed on switch expressions that are exhaustive over a union type's variants"
                );
        }
    }
}
