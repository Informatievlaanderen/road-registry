namespace RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;

using BackOffice.Core;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class RebuildRoadNetworkSnapshotSqsRequestHandler : SqsHandler<RebuildRoadNetworkSnapshotSqsRequest>
{
    public const string Action = "RebuildRoadNetworkSnapshot";

    public RebuildRoadNetworkSnapshotSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(RebuildRoadNetworkSnapshotSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RebuildRoadNetworkSnapshotSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
