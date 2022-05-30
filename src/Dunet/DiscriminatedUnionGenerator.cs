using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Dunet;

public class DiscriminatedUnionGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var source = @"
namespace Dunet.Generated;

public class GeneratedClass
{
    public static void GeneratedMethod()
    {
    }
}";
        context.AddSource("Runs.cs", SourceText.From(source, Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}