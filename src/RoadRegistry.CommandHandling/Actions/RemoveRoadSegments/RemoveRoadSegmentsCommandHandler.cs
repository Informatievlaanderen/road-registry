namespace RoadRegistry.CommandHandling.Actions.RemoveRoadSegments;

using System.Data;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Marten;
using RoadRegistry.RoadNetwork;

public class RemoveRoadSegmentsCommandHandler
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public RemoveRoadSegmentsCommandHandler(
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    public async Task Handle(RemoveRoadSegmentsCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetwork = await Load(command.RoadSegmentIds);

        roadNetwork.RemoveRoadSegments(command.RoadSegmentIds, _roadNetworkIdGenerator, provenance);

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
    }

    private async Task<RoadNetwork> Load(IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadSegmentIds);
        return await _roadNetworkRepository.Load(
            session,
            ids
        );
    }
}
