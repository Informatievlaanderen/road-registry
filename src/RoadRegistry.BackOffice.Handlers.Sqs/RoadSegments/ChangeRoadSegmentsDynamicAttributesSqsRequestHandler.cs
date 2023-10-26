namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class ChangeRoadSegmentsDynamicAttributesSqsRequestHandler : SqsHandler<ChangeRoadSegmentsDynamicAttributesSqsRequest>
{
    public const string Action = "ChangeRoadSegment";

    public ChangeRoadSegmentsDynamicAttributesSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentsDynamicAttributesSqsRequest request)
    {
        return RoadNetwork.Identifier.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentsDynamicAttributesSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
