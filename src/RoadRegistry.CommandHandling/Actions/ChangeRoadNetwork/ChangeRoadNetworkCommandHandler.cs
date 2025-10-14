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
        //set ticket to pending?

        //determine boundingbox from command changes
        Geometry boundingBox = null!;

        var roadNetwork = await _roadNetworkRepository.Load(boundingBox, cancellationToken);

        var changes = new List<IRoadNetworkChange>();
        //TODO-pr convert command changes to domain changes (dit zijn eigenlijk de core objecten)
        //command.Changes
        //= RoadNetworkChangeFactory

        var result = roadNetwork.Change(changes);
        if (result.Accepted)
        {
            await _roadNetworkRepository.Save(roadNetwork, cancellationToken);
        }

        //sluit ticket en extract hier? zie RoadNetworkCommandModule
    }
}
