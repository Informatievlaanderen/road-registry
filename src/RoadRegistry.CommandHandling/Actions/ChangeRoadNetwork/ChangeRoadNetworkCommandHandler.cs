namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using NetTopologySuite.Geometries;
using RoadNetwork;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public ChangeRoadNetworkCommandHandler(IRoadNetworkRepository roadNetworkRepository)
    {
        _roadNetworkRepository = roadNetworkRepository;
    }

    public async Task Handle(ChangeRoadNetworkCommand command, CancellationToken cancellationToken)
    {
        //TODO-pr determine boundingbox from changes
        Geometry boundingBox = null!;

        var roadNetwork = await _roadNetworkRepository.GetScopedRoadNetwork(boundingBox, cancellationToken);


    }
}
