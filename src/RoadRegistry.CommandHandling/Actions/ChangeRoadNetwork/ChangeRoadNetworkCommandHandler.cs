namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Extracts;
using Marten;
using RoadNetwork;
using RoadRegistry.ValueObjects;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;

    public ChangeRoadNetworkCommandHandler(
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IExtractRequests extractRequests)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractRequests = extractRequests;
    }

    public async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.ToRoadNetworkChanges(provenance);

        var roadNetwork = await Load(roadNetworkChanges);
        var downloadId = new DownloadId(command.DownloadId);
        var changeResult = roadNetwork.Change(roadNetworkChanges, downloadId, _roadNetworkIdGenerator);

        if (!changeResult.Problems.HasError())
        {
            await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
            await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);
        }
        else
        {
            //TODO-pr send failed email IExtractUploadFailedEmailClient if external extract (grb) (of GRB logica in aparte lambda handler steken?)
            await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);
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
