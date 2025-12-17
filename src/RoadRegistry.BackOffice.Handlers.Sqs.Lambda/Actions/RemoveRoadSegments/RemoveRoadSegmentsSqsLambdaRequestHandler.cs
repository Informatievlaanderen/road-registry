namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling.Actions.RemoveRoadSegments;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.RoadNetwork;
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
        await Handle(sqsLambdaRequest.Request, sqsLambdaRequest.Provenance, cancellationToken);

        return new object();
    }

    private async Task Handle(RemoveRoadSegmentsCommand command, Provenance provenance, CancellationToken cancellationToken)
    {
        var roadNetwork = await Load(command.RoadSegmentIds);

        roadNetwork.RemoveRoadSegments(command.RoadSegmentIds, _roadNetworkIdGenerator, provenance);

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
    }

    private async Task<RoadNetwork> Load(IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadSegmentIds);
        return await _roadNetworkRepository.Load(
            session,
            ids
        );
    }
}
