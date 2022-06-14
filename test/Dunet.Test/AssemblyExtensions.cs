using System.Reflection;

namespace Dunet.Test;

public static class AssemblyExtensions
{
    public static T Execute<T>(this Assembly assembly, string typeName, string methodName)
    {
        var type = assembly.ExportedTypes.Single(exportedType => exportedType.Name == typeName);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        return (T)method?.Invoke(null, null)!;
    }
}
