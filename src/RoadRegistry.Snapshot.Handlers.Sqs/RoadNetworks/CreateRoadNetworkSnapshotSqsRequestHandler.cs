namespace RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;

using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class CreateRoadNetworkSnapshotSqsRequestHandler : SqsHandler<CreateRoadNetworkSnapshotSqsRequest>
{
    public const string Action = "CreateRoadNetworkSnapshot";

    public CreateRoadNetworkSnapshotSqsRequestHandler(SnapshotSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string? WithAggregateId(CreateRoadNetworkSnapshotSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateRoadNetworkSnapshotSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
