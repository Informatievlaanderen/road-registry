namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeGradeJunctionToGradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeJunctions.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;

public sealed class ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
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
    }

    protected override async Task<object> InnerHandle(ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.GradeJunctionId, scopedRoadNetworkId);

            var result = roadNetwork.ChangeGradeJunctionToGradeSeparatedJunction(
                command.GradeJunctionId,
                command.LowerRoadSegmentId,
                command.UpperRoadSegmentId,
                command.Type,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // Seeded from the grade junction, which brings in the two road segments it is about - and with them everything the
    // action needs to say whether the named segments are its own.
    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, GradeJunctionId gradeJunctionId, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [], [], [gradeJunctionId]));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
