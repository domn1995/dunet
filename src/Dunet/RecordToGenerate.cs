namespace Dunet;

record struct RecordToGenerate(
    string Namespace,
    string Name,
    string Interface,
    List<Parameter> Properties,
    List<Method> Methods
)
{
    public string FullyQualifiedName => $"{Namespace}.{Name}";
}

record Parameter(string Type, string Name);

record Method(string Name, List<Parameter> Parameters);