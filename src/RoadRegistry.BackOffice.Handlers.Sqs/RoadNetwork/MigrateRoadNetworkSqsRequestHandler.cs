namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class MigrateRoadNetworkSqsRequestHandler : SqsHandler<MigrateRoadNetworkSqsRequest>
{
    public const string Action = "MigrateRoadNetwork";

    public MigrateRoadNetworkSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(MigrateRoadNetworkSqsRequest request)
    {
        return Constants.GlobalMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, MigrateRoadNetworkSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
