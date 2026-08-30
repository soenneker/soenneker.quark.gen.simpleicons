using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

/// <summary>
/// Generates the Simple Icons SVG map and dependency-injection support for a consuming project.
/// </summary>
public interface ISimpleIconsWriteRunner
{
    /// <summary>
    /// Generates outputs using the supplied build-task arguments.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The process exit code: zero on success; otherwise nonzero.</returns>
    ValueTask<int> Run(string[] args, CancellationToken cancellationToken);
}
