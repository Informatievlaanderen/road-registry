namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class ChangeRoadSegmentAttributesSqsRequestHandler : SqsHandler<ChangeRoadSegmentAttributesSqsRequest>
{
    public const string Action = "ChangeRoadSegmentAttributes";

    public ChangeRoadSegmentAttributesSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentAttributesSqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentAttributesSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
