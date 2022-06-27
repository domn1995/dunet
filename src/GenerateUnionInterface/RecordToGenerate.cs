namespace Dunet.GenerateUnionInterface;

record RecordToGenerate(
    List<string> Imports,
    string? Namespace,
    string Name,
    string Interface,
    List<Parameter> Properties,
    List<Method> Methods
);

record Parameter(string Type, string Name)
{
    public override string ToString() => $"{Type} {Name}";
}

record Method(string ReturnType, string Name, List<Parameter> Parameters);
