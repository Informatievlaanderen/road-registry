namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentV2;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class SplitRoadSegmentV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<SplitRoadSegmentV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public SplitRoadSegmentV2SqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(SplitRoadSegmentV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var roadSegmentIds = await Handle(sqsLambdaRequest.Request, cancellationToken);

        var responses = new List<ETagResponse>();
        {
            await using var session = Store.LightweightSession();

            foreach (var roadSegmentId in roadSegmentIds)
            {
                var roadSegmentHash = await GetRoadSegmentHash(session, roadSegmentId, cancellationToken);
                responses.Add(new ETagResponse(string.Format(GetRoadSegmentDetailUrlFormat(WellKnownPublicApiVersions.V3), roadSegmentId), roadSegmentHash));
            }
        }

        return responses;
    }

    private async Task<IReadOnlyList<RoadSegmentId>> Handle(SplitRoadSegmentV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);
        IReadOnlyList<RoadSegmentId> roadSegmentIds = [];

        await Store.IdempotentSession(command, async session =>
        {
            // Load the road segment to split together with its surrounding data (connected road nodes and junctions).
            var roadNetwork = await Load(session, [command.RoadSegmentId], scopedRoadNetworkId);

            roadSegmentIds = roadNetwork.SplitRoadSegment(
                command.RoadSegmentId,
                command.CutPosition,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);

            Logger.LogInformation("Split road segment {RoadSegmentId} into {RoadSegmentIds}", command.RoadSegmentId, string.Join(", ", roadSegmentIds));
        }, cancellationToken, Logger);

        return roadSegmentIds;
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], roadSegmentIds, [], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
