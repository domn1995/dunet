using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
/// <remarks>
/// Allows us to use records in a netstandard2.0 projects without getting the following error:
/// Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
sealed class IsExternalInit { }
