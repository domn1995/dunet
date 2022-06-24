namespace Dunet;

record RecordToGenerate(
    List<string> Imports,
    string? Namespace,
    string Name,
    string Interface,
    List<Parameter> Properties,
    List<Method> Methods
)
{
    public string FullyQualifiedName => Namespace is null ? Name : $"{Namespace}.{Name}";
}

record Parameter(string Type, string Name);

record Method(string ReturnType, string Name, List<Parameter> Parameters);
