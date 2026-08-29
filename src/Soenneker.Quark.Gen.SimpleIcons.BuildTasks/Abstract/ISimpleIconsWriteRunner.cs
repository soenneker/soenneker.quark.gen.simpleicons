using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

/// <summary>
/// Defines the simple icons write runner contract.
/// </summary>
public interface ISimpleIconsWriteRunner
{
    /// <summary>
    /// Runs simple Icons Write Runner for the Simple Icons Write Runner.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested value.</returns>
    ValueTask<int> Run(string[] args, CancellationToken cancellationToken);
}
