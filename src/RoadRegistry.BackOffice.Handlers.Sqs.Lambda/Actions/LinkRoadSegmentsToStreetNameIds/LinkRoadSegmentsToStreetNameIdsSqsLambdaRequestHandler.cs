namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.RoadNetwork;
using RoadRegistry.ValueObjects;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class LinkRoadSegmentsToStreetNameIdsSqsLambdaRequestHandler : SqsLambdaHandler<LinkRoadSegmentsToStreetNameIdsSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public LinkRoadSegmentsToStreetNameIdsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.None)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
    }

    protected override async Task<object> InnerHandle(LinkRoadSegmentsToStreetNameIdsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var command = sqsLambdaRequest.Request;

        var roadNetwork = await Load(command.RoadSegmentIds, new ScopedRoadNetworkId(command.TicketId));

        roadNetwork.ChangeStreetNameId(command.RoadSegmentIds, command.OldStreetNameId, command.NewStreetNameId, command.ProvenanceData.ToProvenance());

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);

        return new object();
    }

    private async Task<ScopedRoadNetwork> Load(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        return await _roadNetworkRepository.Load(
            session,
            new RoadNetworkIds([], roadSegmentIds, [], []),
            roadNetworkId
        );
    }
}
