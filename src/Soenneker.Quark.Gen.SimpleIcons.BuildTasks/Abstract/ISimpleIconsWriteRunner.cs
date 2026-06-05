using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

/// <summary>
/// Defines the simple icons write runner contract.
/// </summary>
public interface ISimpleIconsWriteRunner
{
    /// <summary>
    /// Executes the run operation.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    ValueTask<int> Run(string[] args, CancellationToken cancellationToken);
}
