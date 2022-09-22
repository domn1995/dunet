using System.Reflection;

namespace Dunet.Test.Runtime;

public static class AssemblyExtensions
{
    public static T ExecuteStaticMethod<T>(this Assembly assembly, string methodName)
    {
        var method = assembly.DefinedTypes
            .SelectMany(type => type.DeclaredMethods)
            .Single(method => method.Name.Contains(methodName));
        return (T)method?.Invoke(null, null)!;
    }

    public static async Task<T> ExecuteStaticAsyncMethod<T>(this Assembly assembly, string methodName)
    {
        var method = assembly.DefinedTypes
            .SelectMany(type => type.DeclaredMethods)
            .Single(method => method.Name.Contains(methodName));
        return await (Task<T>)method?.Invoke(null, null)!;
    }
}
