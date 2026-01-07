namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.RoadNetwork;
using TicketingService.Abstractions;

public class ChangeRoadNetworkSqsRequestHandler : SqsHandler<ChangeRoadNetworkSqsRequest>
{
    public const string Action = "ChangeRoadNetwork";

    public ChangeRoadNetworkSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadNetworkSqsRequest request)
    {
        return Constants.GlobalMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadNetworkSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
