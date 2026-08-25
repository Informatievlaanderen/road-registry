namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentsByJunction;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using System.Linq;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.ValueObjects;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class SplitRoadSegmentsByJunctionSqsLambdaRequestHandler : MartenSqsLambdaHandler<SplitRoadSegmentsByJunctionSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public SplitRoadSegmentsByJunctionSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
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
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(SplitRoadSegmentsByJunctionSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResult.Summary)
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(SplitRoadSegmentsByJunctionSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, [command.RoadSegmentId1, command.RoadSegmentId2], scopedRoadNetworkId);

            await ValidateInwinningIsCompleted(roadNetwork, [command.RoadSegmentId1, command.RoadSegmentId2], cancellationToken);

            roadNetwork.SplitRoadSegmentsByJunction(
                command.RoadSegmentId1,
                command.RoadSegmentId2,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return new RoadNetworkChangeResult(Problems.None, await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken));
    }

    // A road segment may only be edited once its inwinning is done: one that is still being collected, or that is not
    // being collected at all, is not ours to change yet.
    private async Task ValidateInwinningIsCompleted(ScopedRoadNetwork roadNetwork, IEnumerable<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        // Only the segments the road network actually knows. An identifier that is not there at all is the domain's to
        // report as not found, and 'nietGestart' cannot tell "not being collected" from "does not exist" - so leaving
        // it in here would answer a missing road segment with the wrong problem entirely.
        var knownRoadSegmentIds = roadSegmentIds
            .Where(x => roadNetwork.RoadSegments.TryGetValue(x, out var roadSegment) && !roadSegment.IsRemoved)
            .ToList();

        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus(knownRoadSegmentIds, cancellationToken);

        inwinningsstatus
            .Where(x => x.Value != Inwinningsstatus.Compleet)
            .Aggregate(Problems.None, (problems, x) => problems + new RoadSegmentNotCompletedInwinning(x.Key))
            .ThrowIfError();
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], roadSegmentIds, [], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
