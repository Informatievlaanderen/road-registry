namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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

    public async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = _roadNetworkChangeFactory.Build(command);

        var roadNetwork = await _roadNetworkRepository.Load(roadNetworkChanges);
        var changeResult = roadNetwork.Change(roadNetworkChanges, _roadNetworkIdGenerator);

        if (!changeResult.Problems.HasError())
        {
            await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, provenance, cancellationToken);
        }

        return changeResult;
    }
}
