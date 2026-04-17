namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Exceptions;
using Hosts;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.RoadNetwork.Schema;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class MigrateDryRunRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<MigrateDryRunRoadNetworkSqsLambdaRequest>
{
    private readonly Marten.IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IMediator _mediator;

    public MigrateDryRunRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        Marten.IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ExtractsDbContext extractsDbContext,
        IMediator mediator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Error,
            problemTranslator: WellKnownProblemTranslators.Extract)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _extractsDbContext = extractsDbContext;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(MigrateDryRunRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        try
        {
            await MigrateWithoutSaving(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest, cancellationToken);

            await _mediator.Send(new DataValidationSqsRequest
            {
                TicketId = sqsLambdaRequest.TicketId,
                ProvenanceData = new ProvenanceData(sqsLambdaRequest.Provenance),
                MigrateRoadNetworkSqsRequest = sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest
            }, cancellationToken);

            return new object();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while running migrate dry run with download id {sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.DownloadId}");
            throw;
        }
    }

    private async Task MigrateWithoutSaving(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, new InMemoryRoadNetworkIdGenerator(initialValue: 1_000_000_000));
        if (changeResult.Problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }

        await EnsureAllInwinningRoadSegmentsAreUploaded(command, changeResult, cancellationToken);
    }

    private async Task EnsureAllInwinningRoadSegmentsAreUploaded(MigrateRoadNetworkSqsRequest command, RoadNetworkChangeResult changeResult, CancellationToken cancellationToken)
    {
        var inwinningszone = await _extractsDbContext.Inwinningszones.SingleAsync(x => x.DownloadId == command.DownloadId.ToGuid(), cancellationToken);

        var inwinningRoadSegmentIds = await _extractsDbContext.InwinningRoadSegments
            .Where(x => x.NisCode == inwinningszone.NisCode)
            .Select(x => x.RoadSegmentId)
            .ToListAsync(cancellationToken);
        var modifiedOrRemovedRoadSegmentIds = changeResult.Summary.RoadSegments.Modified
            .Concat(changeResult.Summary.RoadSegments.Removed)
            .Select(x => x.ToInt32())
            .ToList();
        var missingRoadSegmentIds = inwinningRoadSegmentIds.Except(modifiedOrRemovedRoadSegmentIds).ToArray();
        if (missingRoadSegmentIds.Any())
        {
            throw new InvalidOperationException($"Inwinning road segment ids are missing from the upload (most likely bug in FeatureCompare, DownloadId {command.DownloadId}): {string.Join(",", missingRoadSegmentIds)}");
        }
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
}
