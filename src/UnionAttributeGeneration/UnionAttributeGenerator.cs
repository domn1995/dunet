using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Dunet.UnionAttributeGeneration;

[Generator]
public sealed class UnionAttributeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterPostInitializationOutput(
            ctx =>
                ctx.AddSource(
                    $"{UnionAttributeSource.Name}.g.cs",
                    SourceText.From(UnionAttributeSource.SourceCode, Encoding.UTF8)
                )
        );
}
