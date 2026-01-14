namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class RemoveRoadSegmentsSqsLambdaRequestHandler : SqsLambdaHandler<RemoveRoadSegmentsSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public RemoveRoadSegmentsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    protected override async Task<object> InnerHandle(RemoveRoadSegmentsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new object();
    }

    private async Task Handle(RemoveRoadSegmentsSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetwork = await Load(command.RoadSegmentIds, new ScopedRoadNetworkId(command.TicketId));

        roadNetwork.RemoveRoadSegments(command.RoadSegmentIds, _roadNetworkIdGenerator, command.ProvenanceData.ToProvenance());

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
    }

    private async Task<ScopedRoadNetwork> Load(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIdsWithConnectedSegments(session, roadSegmentIds);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }
}
