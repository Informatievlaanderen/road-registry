namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNetwork;
using TicketingService.Abstractions;

public sealed class ChangeRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;

    public ChangeRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IExtractRequests extractRequests,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Complete | TicketingBehavior.Error)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractRequests = extractRequests;
    }

    protected override async Task<object> InnerHandle(ChangeRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var command = sqsLambdaRequest.Request;

        var changeResult = await Handle(command, cancellationToken);

        return new
        {
            Changes = new
            {
                RoadNodes = new
                {
                    Added = changeResult.Changes.RoadNodes.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.RoadNodes.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.RoadNodes.Removed.Select(x => x.ToInt32()).ToList()
                },
                RoadSegments = new
                {
                    Added = changeResult.Changes.RoadSegments.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.RoadSegments.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.RoadSegments.Removed.Select(x => x.ToInt32()).ToList()
                },
                GradeSeparatedJunctions = new
                {
                    Added = changeResult.Changes.GradeSeparatedJunctions.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.GradeSeparatedJunctions.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.GradeSeparatedJunctions.Removed.Select(x => x.ToInt32()).ToList()
                }
            }
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges);
        var downloadId = new DownloadId(command.DownloadId);
        var changeResult = roadNetwork.Change(roadNetworkChanges, downloadId, _roadNetworkIdGenerator);

        if (changeResult.Problems.HasError())
        {
            //TODO-pr send failed email IExtractUploadFailedEmailClient if external extract (grb) (of GRB logica in aparte lambda handler steken?)
            await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);

            //TODO-pr throw ex dat kan worden vertaald naar ticketerror, met juiste identifier, bvb WegsegmentId, WegknoopId, OngelijkGrondseKruisingId in ErrorContext
            //await Ticketing.Error(command.TicketId, new TicketError(errors), cancellationToken);
            throw new NotImplementedException();
        }

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);

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
