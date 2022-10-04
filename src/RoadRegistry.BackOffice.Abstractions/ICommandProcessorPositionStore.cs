namespace RoadRegistry.BackOffice.Abstractions;

using System.Threading;
using System.Threading.Tasks;

public interface ICommandProcessorPositionStore
{
    Task<int?> ReadVersion(string name, CancellationToken cancellationToken);

    Task WriteVersion(string name, int version, CancellationToken cancellationToken);
}
