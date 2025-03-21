using System.Reflection;

namespace Job;

/// <summary>
///     Represents the users module application assembly reference.
/// </summary>
public static class AssemblyReference
{
    /// <summary>
    ///     The assembly.
    /// </summary>
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}