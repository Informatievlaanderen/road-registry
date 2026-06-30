namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
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

        var roadNetwork = await Load(command.RoadSegmentIds, new ScopedRoadNetworkId(command.TicketId));

        roadNetwork.ChangeStreetNameId(command.RoadSegmentIds, command.OldStreetNameId, command.NewStreetNameId, command.ProvenanceData.ToProvenance());

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);

        return new object();
    }

    private async Task<ScopedRoadNetwork> Load(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        return await _roadNetworkRepository.Load(
            session,
            new RoadNetworkIds([], roadSegmentIds, [], []),
            roadNetworkId
        );
    }
}
