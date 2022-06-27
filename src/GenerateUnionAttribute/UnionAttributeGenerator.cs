using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Dunet.UnionAttributeGeneration;

[Generator]
public class UnionAttributeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(
            ctx =>
                ctx.AddSource(
                    "UnionAttribute.g.cs",
                    SourceText.From(UnionAttributeSource.Attribute, Encoding.UTF8)
                )
        );
    }
}
