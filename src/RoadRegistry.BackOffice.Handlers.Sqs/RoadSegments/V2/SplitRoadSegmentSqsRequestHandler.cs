namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using TicketingService.Abstractions;

public class SplitRoadSegmentSqsRequestHandler : SqsHandler<SplitRoadSegmentSqsRequest>
{
    public const string Action = "SplitRoadSegment";

    public SplitRoadSegmentSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(SplitRoadSegmentSqsRequest request)
    {
        return Constants.GlobalRoadNetworkMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SplitRoadSegmentSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
