namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using ChangeRoadNetwork;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNetwork;
using TicketingService.Abstractions;

public sealed class MigrateRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<MigrateRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;
    public MigrateRoadNetworkSqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(MigrateRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Changes = new()
            {
                RoadNodes = new()
                {
                    Added = changeResult.Changes.RoadNodes.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.RoadNodes.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.RoadNodes.Removed.Select(x => x.ToInt32()).ToList()
                },
                RoadSegments = new()
                {
                    Added = changeResult.Changes.RoadSegments.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.RoadSegments.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.RoadSegments.Removed.Select(x => x.ToInt32()).ToList()
                },
                GradeSeparatedJunctions = new()
                {
                    Added = changeResult.Changes.GradeSeparatedJunctions.Added.Select(x => x.ToInt32()).ToList(),
                    Modified = changeResult.Changes.GradeSeparatedJunctions.Modified.Select(x => x.ToInt32()).ToList(),
                    Removed = changeResult.Changes.GradeSeparatedJunctions.Removed.Select(x => x.ToInt32()).ToList()
                }
            }
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        //TODO-pr implement domain Migrate handler
        /*- enkel RoadNetwork opbouwen adv bestaande V2 data
- Add/Modify RoadSegment changes interpreteren als Migrate/Merged events
- RemoveRoadSegment interpreteren als RoadSegmentWasRetiredBecauseOfMigration, indien deel van een merge dan de nieuwe ID er aan toevoegen*/

        throw new NotImplementedException();
        //
        // var roadNetwork = await Load(roadNetworkChanges);
        // var downloadId = new DownloadId(command.DownloadId);
        // var changeResult = roadNetwork.Change(roadNetworkChanges, downloadId, _roadNetworkIdGenerator);
        //
        // if (changeResult.Problems.HasError())
        // {
        //     //TODO-pr send failed email IExtractUploadFailedEmailClient if external extract (grb) (of GRB logica in aparte lambda handler steken?)
        //     await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);

        //     throw new RoadRegistryProblemsException(changeResult.Problems);
        // }

        //await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        //await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);
        ////TODO-pr complete inwinningszone
        //
        // return changeResult;
    }

    private async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges)
    {
        throw new NotImplementedException();
        // await using var session = _store.LightweightSession(IsolationLevel.Snapshot);
        //
        // var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry());
        // return await _roadNetworkRepository.Load(
        //     session,
        //     new RoadNetworkIds(
        //         roadNetworkChanges.RoadNodeIds.Union(ids.RoadNodeIds).ToList(),
        //         roadNetworkChanges.RoadSegmentIds.Union(ids.RoadSegmentIds).ToList(),
        //         roadNetworkChanges.GradeSeparatedJunctionIds.Union(ids.GradeSeparatedJunctionIds).ToList()
        //     )
        // );
    }
}
