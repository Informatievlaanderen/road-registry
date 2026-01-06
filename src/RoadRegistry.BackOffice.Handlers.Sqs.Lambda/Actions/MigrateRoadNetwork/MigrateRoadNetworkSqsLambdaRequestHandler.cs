namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using ChangeRoadNetwork;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNetwork.Events.V2;
using RoadRegistry.RoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class MigrateRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<MigrateRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;
    private readonly ExtractsDbContext _extractsDbContext;

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
        ExtractsDbContext extractsDbContext,
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
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(MigrateRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResult.Summary)
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

         var roadNetwork = await Load(roadNetworkChanges);
         var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);

         if (changeResult.Problems.HasError())
         {
             await _extractRequests.UploadRejectedAsync(command.DownloadId, cancellationToken);

             throw new RoadRegistryProblemsException(changeResult.Problems);
         }

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        await _extractRequests.UploadAcceptedAsync(command.DownloadId, cancellationToken);
        await CompleteInwinningszone(command.DownloadId, cancellationToken);

         return changeResult;
    }

    private async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges)
    {
        //TODO-pr enkel RoadNetwork opbouwen adv bestaande V2 data

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

    private async Task CompleteInwinningszone(DownloadId downloadId, CancellationToken cancellationToken)
    {
        var inwinningszone = await _extractsDbContext.Inwinningszones.SingleAsync(x => x.DownloadId == downloadId.ToGuid(), token: cancellationToken);

        inwinningszone.Completed = true;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
