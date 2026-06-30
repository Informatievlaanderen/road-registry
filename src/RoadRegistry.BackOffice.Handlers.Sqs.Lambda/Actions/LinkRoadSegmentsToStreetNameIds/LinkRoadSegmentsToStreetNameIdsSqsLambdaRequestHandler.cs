namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.ValueObjects;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequestHandler : MartenSqsLambdaHandler<SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            store,
            loggerFactory,
            TicketingBehavior.None)
    {
        _roadNetworkRepository = roadNetworkRepository;
    }

    protected override async Task<object> InnerHandle(SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var command = sqsLambdaRequest.Request;
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.RoadSegmentIds, scopedRoadNetworkId);

            roadNetwork.ChangeStreetNameId(command.RoadSegmentIds, command.OldStreetNameId, command.NewStreetNameId, command.ProvenanceData.ToProvenance());

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return new object();
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        return await _roadNetworkRepository.Load(
            session,
            new RoadNetworkIds([], roadSegmentIds, [], []),
            roadNetworkId
        );
    }
}
