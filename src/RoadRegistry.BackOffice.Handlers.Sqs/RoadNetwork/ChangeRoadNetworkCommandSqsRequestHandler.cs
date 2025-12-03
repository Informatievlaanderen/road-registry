namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadSegments;
using TicketingService.Abstractions;

public class ChangeRoadNetworkCommandSqsRequestHandler : SqsHandler<ChangeRoadNetworkCommandSqsRequest>
{
    public const string Action = "ChangeRoadNetwork";

    public ChangeRoadNetworkCommandSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadNetworkCommandSqsRequest request)
    {
        return RoadRegistry.RoadNetwork.RoadNetwork.GlobalIdentifier;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadNetworkCommandSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
