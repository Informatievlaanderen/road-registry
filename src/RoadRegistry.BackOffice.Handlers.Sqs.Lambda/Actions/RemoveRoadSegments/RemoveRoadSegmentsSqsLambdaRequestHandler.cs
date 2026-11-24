namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class RemoveRoadSegmentsSqsLambdaRequestHandler : MartenSqsLambdaHandler<RemoveRoadSegmentsSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public RemoveRoadSegmentsSqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(RemoveRoadSegmentsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(RemoveRoadSegmentsSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.RoadSegmentIds, scopedRoadNetworkId);

            var problems = await ValidateInwinningIsCompleted(roadNetwork, command.RoadSegmentIds, cancellationToken);
            problems.ThrowIfError();

            roadNetwork.RemoveRoadSegments(command.RoadSegmentIds, _roadNetworkIdGenerator, command.ProvenanceData.ToProvenance());

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        // Read back rather than carried out of the domain: a request that removed nothing raises no change at all, and
        // then there is no summary to report - the same answer the status changes give.
        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        return await _roadNetworkRepository.LoadWithConnectedSegments(session, roadSegmentIds, roadNetworkId);
    }

    // VAL-9: every road segment the request names has to have its inwinning done. The aggregate says for itself
    // whether it is a V2 road segment, which answers 'nietGestart', but not whether a municipality is collecting it
    // again - a road segment that was ingewonnen once and is now 'locked' carries its V2 attributes all the same. That
    // is only recorded in the extracts database, so it is asked here, the same as for every other editing action.
    private async Task<Problems> ValidateInwinningIsCompleted(ScopedRoadNetwork roadNetwork, IEnumerable<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        // Only the segments the road network actually knows. An identifier that is not there at all is the domain's to
        // report as not found, and 'nietGestart' cannot tell "not being collected" from "does not exist" - so leaving
        // it in here would answer a missing road segment with the wrong problem entirely.
        var knownRoadSegmentIds = roadSegmentIds
            .Where(x => roadNetwork.RoadSegments.TryGetValue(x, out var roadSegment) && !roadSegment.IsRemoved)
            .ToList();

        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus(knownRoadSegmentIds, roadNetwork.SelectMigratedRoadSegmentIds(), cancellationToken);

        return inwinningsstatus
            .Where(x => x.Value != Inwinningsstatus.Compleet)
            .Aggregate(Problems.None, (problems, x) => problems + new RoadSegmentNotCompletedInwinning(x.Key));
    }
}
