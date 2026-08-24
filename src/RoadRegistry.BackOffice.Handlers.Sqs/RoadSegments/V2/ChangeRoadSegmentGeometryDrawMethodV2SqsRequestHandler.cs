namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using TicketingService.Abstractions;

public class ChangeRoadSegmentGeometryDrawMethodV2SqsRequestHandler : SqsHandler<ChangeRoadSegmentGeometryDrawMethodV2SqsRequest>
{
    public const string Action = "ChangeRoadSegmentGeometryDrawMethod";

    public ChangeRoadSegmentGeometryDrawMethodV2SqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentGeometryDrawMethodV2SqsRequest request)
    {
        return Constants.GlobalRoadNetworkMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentGeometryDrawMethodV2SqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
