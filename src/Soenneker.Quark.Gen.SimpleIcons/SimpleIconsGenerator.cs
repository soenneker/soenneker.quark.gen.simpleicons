using Microsoft.CodeAnalysis;

namespace Soenneker.Quark.Gen.SimpleIcons;

/// <summary>
/// Provides the analyzer entry point for the Simple Icons build-time generator package.
/// </summary>
[Generator]
public sealed class SimpleIconsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the analyzer entry point. Simple Icons source generation is performed by the package's MSBuild task.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
