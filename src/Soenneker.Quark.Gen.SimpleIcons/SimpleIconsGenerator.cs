using Microsoft.CodeAnalysis;

namespace Soenneker.Quark.Gen.SimpleIcons;

/// <summary>
/// Represents the simple icons generator.
/// </summary>
[Generator]
public sealed class SimpleIconsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the Simple Icons Generator so it is ready for use.
    /// </summary>
    /// <param name="context">HTTP context containing the Authorization header.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
