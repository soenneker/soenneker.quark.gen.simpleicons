using Microsoft.CodeAnalysis;

namespace Soenneker.Quark.Gen.SimpleIcons;

/// <summary>
/// Represents the simple icons generator.
/// </summary>
[Generator]
public sealed class SimpleIconsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Executes the initialize operation.
    /// </summary>
    /// <param name="context">The context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
