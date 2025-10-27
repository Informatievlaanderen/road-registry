namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using BackOffice.Core;
using RoadNetwork;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly RoadNetworkChangesFactory _roadNetworkChangeFactory;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public ChangeRoadNetworkCommandHandler(
        IRoadNetworkRepository roadNetworkRepository,
        RoadNetworkChangesFactory roadNetworkChangeFactory,
        IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkChangeFactory = roadNetworkChangeFactory;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    public async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkCommand command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = await _roadNetworkChangeFactory.Build(command);

        var roadNetwork = await _roadNetworkRepository.Load(roadNetworkChanges, cancellationToken);
        var changeResult = roadNetwork.Change(roadNetworkChanges, _roadNetworkIdGenerator);

        if (!changeResult.Problems.HasError())
        {
            //TODO-pr save command.Provenance in event metadata?
            await _roadNetworkRepository.Save(roadNetwork, cancellationToken);
        }

        return changeResult;
    }
}
