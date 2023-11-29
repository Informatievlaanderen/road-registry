namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class ChangeRoadSegmentOutlineGeometrySqsRequestHandler : SqsHandler<ChangeRoadSegmentOutlineGeometrySqsRequest>
{
    public const string Action = "ChangeRoadSegmentOutlineGeometry";

    public ChangeRoadSegmentOutlineGeometrySqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentOutlineGeometrySqsRequest request)
    {
        return RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(request.Request.RoadSegmentId);
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentOutlineGeometrySqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
