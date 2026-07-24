namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentsByJunctionV2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class SplitRoadSegmentsByJunctionV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<SplitRoadSegmentsByJunctionV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public SplitRoadSegmentsByJunctionV2SqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(SplitRoadSegmentsByJunctionV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var command = sqsLambdaRequest.Request;

        await Handle(command, cancellationToken);

        var responses = new List<ETagResponse>();
        {
            await using var session = Store.LightweightSession();

            // The split result is recovered from the aggregate state (populated by RoadSegmentWasSplit) of both original
            // segments, so a retry that skips the mutation still yields the same response.
            foreach (var originalRoadSegmentId in new[] { command.RoadSegmentId1, command.RoadSegmentId2 })
            {
                var roadSegment = await session.LoadAsync(originalRoadSegmentId, cancellationToken);
                if (roadSegment is null)
                {
                    throw new RoadSegmentNotFoundException();
                }

                var roadSegmentIds = roadSegment.LastSplitIntoRoadSegmentIds ?? [];

                foreach (var roadSegmentId in roadSegmentIds)
                {
                    var roadSegmentHash = await GetRoadSegmentHash(session, roadSegmentId, cancellationToken);
                    responses.Add(new ETagResponse(string.Format(GetRoadSegmentDetailUrlFormat(WellKnownPublicApiVersions.V3), roadSegmentId), roadSegmentHash));
                }
            }
        }

        return responses;
    }

    private Task Handle(SplitRoadSegmentsByJunctionV2SqsRequest command, CancellationToken cancellationToken)
    {
        return Store.IdempotentSession(command, async session =>
        {
            var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);
            var roadNetwork = await Load(session, [command.RoadSegmentId1, command.RoadSegmentId2], scopedRoadNetworkId);

            var roadSegmentIds = roadNetwork.SplitRoadSegmentsByJunction(
                command.RoadSegmentId1,
                command.RoadSegmentId2,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);

            Logger.LogInformation("Split road segments {RoadSegmentId1} and {RoadSegmentId2} by junction into {RoadSegmentIds}", command.RoadSegmentId1, command.RoadSegmentId2, string.Join(",", roadSegmentIds));
        }, cancellationToken, Logger);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], roadSegmentIds, [], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
