namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using ChangeRoadNetwork;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;

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

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.SummaryOfLastChange is null
            ? await ChangeRoadNetwork(command, roadNetwork, roadNetworkChanges, cancellationToken)
            : new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange);

        await _extractRequests.UploadAcceptedAsync(command.DownloadId, cancellationToken);
        await CompleteInwinningszone(command.DownloadId, cancellationToken);

        return changeResult;
    }

    private async Task<RoadNetworkChangeResult> ChangeRoadNetwork(MigrateRoadNetworkSqsRequest command, ScopedRoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);
        if (changeResult.Problems.HasError())
        {
            await _extractRequests.UploadRejectedAsync(command.DownloadId, cancellationToken);

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        return changeResult;
    }

    private async Task<ScopedRoadNetwork> Load(RoadNetworkChanges roadNetworkChanges, ScopedRoadNetworkId roadNetworkId)
    {
        if (!roadNetworkChanges.Any())
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry(), ids: roadNetworkChanges.Ids, onlyV2: true);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }

    private async Task CompleteInwinningszone(DownloadId downloadId, CancellationToken cancellationToken)
    {
        var inwinningszone = await EntityFrameworkQueryableExtensions.SingleAsync(_extractsDbContext.Inwinningszones, x => x.DownloadId == downloadId.ToGuid(), cancellationToken);

        inwinningszone.Completed = true;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
