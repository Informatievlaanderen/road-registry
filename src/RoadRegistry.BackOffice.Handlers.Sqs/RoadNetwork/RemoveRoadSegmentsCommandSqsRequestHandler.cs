namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class RemoveRoadSegmentsCommandSqsRequestHandler : SqsHandler<RemoveRoadSegmentsCommandSqsRequest>
{
    public const string Action = "DeleteRoadSegments";

    public RemoveRoadSegmentsCommandSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(RemoveRoadSegmentsCommandSqsRequest request)
    {
        return RoadRegistry.RoadNetwork.RoadNetwork.GlobalIdentifier;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RemoveRoadSegmentsCommandSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
