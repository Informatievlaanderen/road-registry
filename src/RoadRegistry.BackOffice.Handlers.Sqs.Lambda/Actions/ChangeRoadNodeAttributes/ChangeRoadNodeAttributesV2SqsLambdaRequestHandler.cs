namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNodeAttributes;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;

public sealed class ChangeRoadNodeAttributesV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadNodeAttributesV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public ChangeRoadNodeAttributesV2SqsLambdaRequestHandler(
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
            loggerFactory)
    {
        _roadNetworkRepository = roadNetworkRepository;
    }

    protected override async Task<object> InnerHandle(ChangeRoadNodeAttributesV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeRoadNodeAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNodeIds = command.Groups.SelectMany(x => x.RoadNodeIds).Distinct().ToList();
            var roadNetwork = await Load(session, roadNodeIds, scopedRoadNetworkId);

            var provenance = command.ProvenanceData.ToProvenance();

            // Attribute-only edit: the geometry is left null so it stays what it is, and the road node type is never
            // named by hand - it follows from the network.
            var changes = command.Groups
                .SelectMany(group => group.RoadNodeIds.Select(roadNodeId => new ModifyRoadNodeChange
                {
                    RoadNodeId = roadNodeId,
                    Grensknoop = group.Grensknoop
                }))
                .ToList();

            var result = roadNetwork.ModifyRoadNodeAttributes(changes, provenance, Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadNodeId> roadNodeIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds(roadNodeIds, [], [], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
