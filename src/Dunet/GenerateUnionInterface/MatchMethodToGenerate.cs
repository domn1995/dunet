using Microsoft.CodeAnalysis;

namespace Dunet.GenerateUnionInterface;

record MatchMethodToGenerate(
    List<string> Imports,
    Accessibility Accessibility,
    string? Namespace,
    string Interface,
    List<MatchMethodParameter> Parameters
);

record MatchMethodParameter(string Type)
{
    public string Name => Type.ToMethodParameterCase();
}
