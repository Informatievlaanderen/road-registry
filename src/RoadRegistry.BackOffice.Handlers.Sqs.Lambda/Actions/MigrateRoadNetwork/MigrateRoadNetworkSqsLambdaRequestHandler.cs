namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using ChangeRoadNetwork;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.DutchTranslations;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class MigrateRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<MigrateRoadNetworkSqsLambdaRequest>
{
    private readonly Marten.IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public MigrateRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        Marten.IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ExtractsDbContext extractsDbContext,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Complete | TicketingBehavior.Error,
            problemTranslator: WellKnownProblemTranslators.Extract)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(MigrateRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        try
        {
            var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

            return new ChangeRoadNetworkTicketResult
            {
                Summary = new RoadNetworkChangedSummary(changeResult.Summary)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while migrating roadnetwork with download id {sqsLambdaRequest.Request.DownloadId}");
            throw;
        }
    }

    private async Task<RoadNetworkChangeResult> Handle(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.SummaryOfLastChange is null
            ? await ChangeRoadNetwork(command, roadNetwork, roadNetworkChanges, cancellationToken)
            : new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange);

        await _extractsDbContext.UploadAcceptedAsync(command.UploadId, cancellationToken);
        await CompleteInwinningStatuses(command.DownloadId, changeResult, cancellationToken);

        return changeResult;
    }

    private async Task<RoadNetworkChangeResult> ChangeRoadNetwork(MigrateRoadNetworkSqsRequest command, ScopedRoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator, Logger);
        if (changeResult.Problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

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

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry(), ids: roadNetworkChanges.Ids);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }

    private async Task CompleteInwinningStatuses(DownloadId downloadId, RoadNetworkChangeResult changeResult, CancellationToken cancellationToken)
    {
        var inwinningszone = await _extractsDbContext.Inwinningszones.SingleAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        inwinningszone.Completed = true;

        var inwinningRoadSegments = await _extractsDbContext.InwinningRoadSegments
            .Where(x => x.NisCode == inwinningszone.NisCode)
            .ToListAsync(cancellationToken);
        foreach (var inwinningRoadSegment in inwinningRoadSegments)
        {
            inwinningRoadSegment.Completed = true;
        }

        _extractsDbContext.InwinningRoadSegments.AddRange(changeResult.Summary.RoadSegments.Added.Select(roadSegmentId => new InwinningRoadSegment
        {
            NisCode = inwinningszone.NisCode,
            RoadSegmentId = roadSegmentId,
            Completed = true
        }));

        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
