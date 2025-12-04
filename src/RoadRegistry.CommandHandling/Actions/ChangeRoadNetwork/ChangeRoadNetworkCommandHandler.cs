namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Marten;
using RoadNetwork;
using RoadRegistry.ValueObjects;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public ChangeRoadNetworkCommandHandler(
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    public async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.ToRoadNetworkChanges(provenance);

        var roadNetwork = await Load(roadNetworkChanges);
        var changeResult = roadNetwork.Change(roadNetworkChanges, DownloadId.FromValue(command.DownloadId), _roadNetworkIdGenerator);

        if (!changeResult.Problems.HasError())
        {
            await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        }

        return changeResult;
    }

    private async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry());
        return await _roadNetworkRepository.Load(
            session,
            new RoadNetworkIds(
                roadNetworkChanges.RoadNodeIds.Union(ids.RoadNodeIds).ToList(),
                roadNetworkChanges.RoadSegmentIds.Union(ids.RoadSegmentIds).ToList(),
                roadNetworkChanges.GradeSeparatedJunctionIds.Union(ids.GradeSeparatedJunctionIds).ToList()
            )
        );
    }
}
