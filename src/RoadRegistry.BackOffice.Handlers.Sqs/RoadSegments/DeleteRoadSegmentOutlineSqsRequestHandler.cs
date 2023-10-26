namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class DeleteRoadSegmentOutlineSqsRequestHandler : SqsHandler<DeleteRoadSegmentOutlineSqsRequest>
{
    public const string Action = "DeleteRoadSegmentOutline";

    public DeleteRoadSegmentOutlineSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(DeleteRoadSegmentOutlineSqsRequest request)
    {
        return RoadNetwork.Identifier.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, DeleteRoadSegmentOutlineSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId },
            { ObjectIdKey, sqsRequest.Request.WegsegmentId.ToString() }
        };
    }
}
