using System.Reflection;

namespace Dunet.Test.Runtime;

/// <summary>
/// Provides extension methods for executing methods within an assembly.
/// </summary>
/// <remarks>
/// This helps Dunet ensure that match methods are working correctly.
/// </remarks>
internal static class AssemblyExtensions
{
    public static T? ExecuteStaticMethod<T>(this Assembly assembly, string methodName) =>
        (T?)
            assembly
                .DefinedTypes.SelectMany(type => type.DeclaredMethods)
                .FirstOrDefault(method => method.Name.Contains(methodName))
                ?.Invoke(null, null);

    public static T? ExecuteStaticAsyncMethod<T>(this Assembly assembly, string methodName)
    {
        var task =
            assembly
                .DefinedTypes.SelectMany(type => type.DeclaredMethods)
                .FirstOrDefault(method => method.Name.Contains(methodName))
                ?.Invoke(null, null) as Task<T>;

        return task switch
        {
            not null => task.GetAwaiter().GetResult(),
            _ => default,
        };
    }
}
