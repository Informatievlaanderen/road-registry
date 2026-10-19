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
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class MigrateDryRunRoadNetworkSqsLambdaRequestHandler : MartenSqsLambdaHandler<MigrateDryRunRoadNetworkSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IMediator _mediator;

    public MigrateDryRunRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
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
            store,
            loggerFactory,
            TicketingBehavior.Error,
            problemTranslator: WellKnownProblemTranslators.Extract)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _extractsDbContext = extractsDbContext;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(MigrateDryRunRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        try
        {
            await MigrateWithoutSaving(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest, sqsLambdaRequest.Request.OnlyDryRun, cancellationToken);

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

    private async Task MigrateWithoutSaving(MigrateRoadNetworkSqsRequest command, bool onlyDryRun, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, new InMemoryRoadNetworkIdGenerator(initialValue: 1_000_000_000), Logger);
        var problems = changeResult.Problems;
        if (!problems.HasError() && onlyDryRun)
        {
            problems += new Error(ProblemCode.Upload.DryRunSuccessful);
        }

        if (problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            throw new RoadRegistryProblemsException(problems);
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
        var missingRoadSegmentIds = inwinningRoadSegmentIds
            .Except(modifiedOrRemovedRoadSegmentIds)
            .Except(command.IdenticalRoadSegmentIds.Select(x => x.ToInt32()))
            .Select(x => new RoadSegmentId(x))
            .ToArray();
        if (!missingRoadSegmentIds.Any())
        {
            return;
        }

        // A road segment the inwinning is still waiting on that is already a V2 one was collected before - by an
        // earlier upload for this zone, or by a V2 edit. There is nothing left for this upload to carry about it, so
        // it is not missing from it: it is locked only because this inwinning still lists it, and completing the
        // inwinning is what unlocks it. A segment that is still V1 is the one that points at an upload losing it.
        var alreadyCollectedRoadSegmentIds = await SelectMigratedRoadSegmentIds(command, missingRoadSegmentIds, cancellationToken);
        if (alreadyCollectedRoadSegmentIds.Any())
        {
            Logger.LogInformation("Inwinning road segment ids are not in the upload but are already ingewonnen, so they are completed along with it (DownloadId {DownloadId}): {RoadSegmentIds}", command.DownloadId, string.Join(",", alreadyCollectedRoadSegmentIds));
        }

        var notUploadedRoadSegmentIds = missingRoadSegmentIds.Except(alreadyCollectedRoadSegmentIds).ToArray();
        if (notUploadedRoadSegmentIds.Any())
        {
            throw new InvalidOperationException($"Inwinning road segment ids are missing from the upload (most likely bug in FeatureCompare, DownloadId {command.DownloadId}): {string.Join(",", notUploadedRoadSegmentIds)}");
        }
    }

    // Whether a road segment is already V2 is the road network's to answer. These are loaded on their own rather
    // than read off the network loaded for the migration: that one is scoped to what the upload touches, which is
    // exactly what these segments are not part of.
    private async Task<IReadOnlyCollection<RoadSegmentId>> SelectMigratedRoadSegmentIds(MigrateRoadNetworkSqsRequest command, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        var roadNetwork = await _roadNetworkRepository.Load(session, new RoadNetworkIds([], roadSegmentIds, [], []), new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        return await roadNetwork.SelectMigratedRoadSegmentIds()(roadSegmentIds, cancellationToken);
    }

    private async Task<ScopedRoadNetwork> Load(RoadNetworkChanges roadNetworkChanges, ScopedRoadNetworkId roadNetworkId)
    {
        if (!roadNetworkChanges.Any())
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        MultiPolygon geometry;
        using (var _ = Logger.TimeAction(action: "RoadNetworkChanges.BuildScopeGeometry"))
        {
            geometry = roadNetworkChanges.BuildScopeGeometry();
        }

        RoadNetworkIds ids;
        using (var _ = Logger.TimeAction(action: "RoadNetworkRepository.GetUnderlyingIds"))
        {
            ids = await _roadNetworkRepository.GetUnderlyingIds(session, geometry, ids: roadNetworkChanges.Ids);
        }

        using (var _ = Logger.TimeAction(action: "RoadNetworkRepository.Load"))
        {
            return await _roadNetworkRepository.Load(
                session,
                ids,
                roadNetworkId
            );
        }
    }
}
