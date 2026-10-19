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

        // Only worth asking once the changes themselves hold up: comparing what the upload touched against what the
        // inwinning is waiting on says nothing about an upload that is already being rejected.
        if (!problems.HasError())
        {
            problems += await ValidateAllInwinningRoadSegmentsAreUploaded(command, changeResult, cancellationToken);
        }

        // A real problem wins over the signal that the simulation ran to the end, so this comes after the two checks.
        if (!problems.HasError() && onlyDryRun)
        {
            problems += new Error(ProblemCode.Upload.DryRunSuccessful);
        }

        if (problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            throw new RoadRegistryProblemsException(problems);
        }
    }

    private async Task<Problems> ValidateAllInwinningRoadSegmentsAreUploaded(MigrateRoadNetworkSqsRequest command, RoadNetworkChangeResult changeResult, CancellationToken cancellationToken)
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
            return Problems.None;
        }

        // A road segment the inwinning is still waiting on that is already a V2 one was collected before - by an
        // earlier upload for this zone, or by a V2 edit - and one that has since been removed has nothing left to
        // collect at all. Neither is missing from this upload: they are locked only because this inwinning still
        // lists them, and completing the inwinning is what unlocks them. A segment that is still V1 and still there
        // is the one that points at an upload losing it.
        var settledRoadSegmentIds = await SelectSettledRoadSegmentIds(command, missingRoadSegmentIds);
        if (settledRoadSegmentIds.Any())
        {
            Logger.LogInformation("Inwinning road segment ids are not in the upload but are already ingewonnen or removed, so they are completed along with it (DownloadId {DownloadId}): {RoadSegmentIds}", command.DownloadId, string.Join(",", settledRoadSegmentIds));
        }

        var notUploadedRoadSegmentIds = missingRoadSegmentIds.Except(settledRoadSegmentIds).ToArray();
        if (notUploadedRoadSegmentIds.Any())
        {
            // This points at the upload losing a road segment somewhere before it got here rather than at anything
            // the caller did, so it is logged as an error for us to act on - with the full list, which the reported
            // problem caps to keep the message readable.
            Logger.LogError("Inwinning road segment ids are missing from the upload (most likely bug in FeatureCompare, DownloadId {DownloadId}): {RoadSegmentIds}", command.DownloadId, string.Join(",", notUploadedRoadSegmentIds));

            return Problems.Single(new InwinningRoadSegmentsMissingFromUpload(notUploadedRoadSegmentIds));
        }

        return Problems.None;
    }

    // The road segments an inwinning has nothing left to ask of: the ones that are already V2, and the ones that
    // are gone. Both are the road network's to answer, so they are loaded from it - on their own rather than read
    // off the network loaded for the migration, which is scoped to what the upload touches, exactly what these
    // segments are not part of. An identifier the road network does not know at all is left out of this: nothing
    // can be said about it, so it stays reported.
    private async Task<IReadOnlyCollection<RoadSegmentId>> SelectSettledRoadSegmentIds(MigrateRoadNetworkSqsRequest command, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        var roadNetwork = await _roadNetworkRepository.Load(session, new RoadNetworkIds([], roadSegmentIds, [], []), new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        return roadSegmentIds
            .Where(x => roadNetwork.RoadSegments.TryGetValue(x, out var roadSegment) && (roadSegment.HasMigrated() || roadSegment.IsRemoved))
            .ToList();
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
