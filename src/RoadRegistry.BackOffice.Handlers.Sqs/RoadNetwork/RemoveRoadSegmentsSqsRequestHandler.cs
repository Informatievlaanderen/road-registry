namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class RemoveRoadSegmentsSqsRequestHandler : SqsHandler<RemoveRoadSegmentsSqsRequest>
{
    public const string Action = "RemoveRoadSegments";

    public RemoveRoadSegmentsSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(RemoveRoadSegmentsSqsRequest request)
    {
        return RoadRegistry.RoadNetwork.RoadNetwork.GlobalIdentifier;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RemoveRoadSegmentsSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
