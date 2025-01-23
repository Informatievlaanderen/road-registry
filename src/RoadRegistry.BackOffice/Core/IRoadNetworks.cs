namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using Framework;

public interface IRoadNetworks
{
    Task<RoadNetwork> Get(CancellationToken cancellationToken);
    Task<RoadNetwork> Get(StreamName streamName, CancellationToken cancellationToken);
    Task<(RoadNetwork, int)> GetWithVersion(bool restoreSnapshot, ProcessMessageHandler cancelMessageProcessing, CancellationToken cancellationToken);
    Task<RoadNetwork> ForOutlinedRoadSegment(RoadSegmentId roadSegmentId, CancellationToken cancellationToken);
}
