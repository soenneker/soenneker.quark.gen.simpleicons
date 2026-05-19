using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

public interface ISimpleIconsWriteRunner
{
    ValueTask<int> Run(string[] args, CancellationToken cancellationToken);
}
