namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadNetwork;
using RoadRegistry.ValueObjects;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public ChangeRoadNetworkCommandHandler(
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    public async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.ToRoadNetworkChanges(provenance);

        var roadNetwork = await _roadNetworkRepository.Load(roadNetworkChanges);
        var changeResult = roadNetwork.Change(roadNetworkChanges, DownloadId.FromValue(command.DownloadId), _roadNetworkIdGenerator);

        if (!changeResult.Problems.HasError())
        {
            await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        }

        return changeResult;
    }
}
