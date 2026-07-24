namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class SplitRoadSegmentsByJunctionV2SqsRequestHandler : SqsHandler<SplitRoadSegmentsByJunctionV2SqsRequest>
{
    public const string Action = "SplitRoadSegmentsByJunctionV2";

    public SplitRoadSegmentsByJunctionV2SqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(SplitRoadSegmentsByJunctionV2SqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SplitRoadSegmentsByJunctionV2SqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
