namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworks
{
    Task<RoadNetwork> Get(CancellationToken cancellationToken);
    Task<RoadNetwork> Get(bool restoreSnapshot, ProcessMessageHandler cancelMessageProcessing, CancellationToken cancellationToken);
    Task<(RoadNetwork, int)> GetWithVersion(CancellationToken cancellationToken);
    Task<(RoadNetwork, int)> GetWithVersion(bool restoreSnapshot, ProcessMessageHandler cancelMessageProcessing, CancellationToken cancellationToken);
}
