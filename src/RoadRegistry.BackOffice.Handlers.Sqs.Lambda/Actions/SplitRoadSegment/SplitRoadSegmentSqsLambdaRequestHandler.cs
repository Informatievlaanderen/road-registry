namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegment;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using System.Collections.Generic;
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

public sealed class SplitRoadSegmentSqsLambdaRequestHandler : MartenSqsLambdaHandler<SplitRoadSegmentSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public SplitRoadSegmentSqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(SplitRoadSegmentSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResult.Summary)
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(SplitRoadSegmentSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            await ValidateInwinningIsCompleted([command.RoadSegmentId], cancellationToken);

            var roadNetwork = await Load(session, [command.RoadSegmentId], scopedRoadNetworkId);

            roadNetwork.SplitRoadSegment(
                command.RoadSegmentId,
                command.CutPosition,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        // The summary is recovered from the persisted scoped road network aggregate (populated by the change-summary
        // event) rather than from the domain call, so a retry that skips the mutation still yields the same response.
        await using var readSession = Store.LightweightSession();
        var scopedRoadNetwork = await readSession.LoadAsync(scopedRoadNetworkId, cancellationToken);
        return new RoadNetworkChangeResult(Problems.None, scopedRoadNetwork.SummaryOfLastChange!);
    }

    // A road segment may only be edited once its inwinning is done: one that is still being collected, or that is not
    // being collected at all, is not ours to change yet.
    private async Task ValidateInwinningIsCompleted(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus(roadSegmentIds, cancellationToken);

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
