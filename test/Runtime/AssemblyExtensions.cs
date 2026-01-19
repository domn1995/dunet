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
    extension(Assembly self)
    {
        public T? ExecuteStaticMethod<T>(string methodName) =>
            (T?)
                self
                    .DefinedTypes.SelectMany(type => type.DeclaredMethods)
                    .FirstOrDefault(method => method.Name.Contains(methodName))
                    ?.Invoke(null, null);

        public T? ExecuteStaticAsyncMethod<T>(string methodName)
        {
            var task =
                self.DefinedTypes.SelectMany(type => type.DeclaredMethods)
                    .FirstOrDefault(method => method.Name.Contains(methodName))
                    ?.Invoke(null, null) as Task<T>;

            return task switch
            {
                not null => task.GetAwaiter().GetResult(),
                _ => default,
            };
        }
    }
}
