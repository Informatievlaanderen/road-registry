namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CorrectRoadSegmentFromRealizedToPlanned;

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
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(CorrectRoadSegmentFromRealizedToPlannedV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            // A realized segment is knotted into the network, so everything this action touches is reachable through
            // its road nodes: the segment itself, the nodes at either end, the segments hanging off them - which is
            // what decides whether a node survives - and the junctions it takes part in.
            var ids = await _roadNetworkRepository.GetUnderlyingIdsWithConnectedSegments(session, [command.RoadSegmentId]);
            var roadNetwork = await _roadNetworkRepository.Load(session, ids, scopedRoadNetworkId);

            var problems = await ValidateInwinningIsCompleted(roadNetwork, command.RoadSegmentId, cancellationToken);
            problems.ThrowIfError();

            var result = roadNetwork.CorrectRoadSegmentFromRealizedToPlanned(
                command.RoadSegmentId,
                command.MayModifyMeasuredRoadSegments,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // A road segment may only be edited once its inwinning is done: one that is still being collected, or that is not
    // being collected at all, is not ours to change yet.
    private async Task<Problems> ValidateInwinningIsCompleted(ScopedRoadNetwork roadNetwork, RoadSegmentId roadSegmentId, CancellationToken cancellationToken)
    {
        // An identifier the road network does not know is the domain's to report as not found, and 'nietGestart' cannot
        // tell "not being collected" from "does not exist" - so checking it here would answer a missing road segment
        // with the wrong problem entirely.
        if (!roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Problems.None;
        }

        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus([roadSegmentId], cancellationToken);

        return inwinningsstatus
            .Where(x => x.Value != Inwinningsstatus.Compleet)
            .Aggregate(Problems.None, (problems, x) => problems + new RoadSegmentNotCompletedInwinning(x.Key));
    }
}
