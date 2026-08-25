namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometryDrawMethod;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly ExtractsDbContext _extractsDbContext;

    public ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ExtractsDbContext extractsDbContext,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            store,
            loggerFactory)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeRoadSegmentGeometryDrawMethodV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            // The draw method never moves the geometry, so the topology stays what it is: only the named segments are
            // loaded - no connected segments, no search by geometry.
            var roadSegmentIds = command.Groups.SelectMany(x => x.RoadSegmentIds).Distinct().ToList();
            var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], roadSegmentIds, [], []));
            var roadNetwork = await _roadNetworkRepository.Load(session, ids, scopedRoadNetworkId);

            var problems = await ValidateInwinningIsCompleted(roadNetwork, roadSegmentIds, cancellationToken);
            problems.ThrowIfError();

            var changes = command.Groups
                .SelectMany(group => group.RoadSegmentIds.Select(roadSegmentId => new ChangeRoadSegmentGeometryDrawMethodChange
                {
                    RoadSegmentId = roadSegmentId,
                    GeometryDrawMethod = group.GeometryDrawMethod
                }))
                .ToList();

            var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod(changes, command.ProvenanceData.ToProvenance(), Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // A road segment may only be edited once its inwinning is done: one that is still being collected, or that is not
    // being collected at all, is not ours to change yet.
    private async Task<Problems> ValidateInwinningIsCompleted(ScopedRoadNetwork roadNetwork, IEnumerable<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        // Only the segments the road network actually knows. An identifier that is not there at all is the domain's to
        // report as not found, and 'nietGestart' cannot tell "not being collected" from "does not exist" - so leaving
        // it in here would answer a missing road segment with the wrong problem entirely.
        var knownRoadSegmentIds = roadSegmentIds
            .Where(x => roadNetwork.RoadSegments.TryGetValue(x, out var roadSegment) && !roadSegment.IsRemoved)
            .ToList();

        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus(knownRoadSegmentIds, cancellationToken);

        return inwinningsstatus
            .Where(x => x.Value != Inwinningsstatus.Compleet)
            .Aggregate(Problems.None, (problems, x) => problems + new RoadSegmentNotCompletedInwinning(x.Key));
    }
}
