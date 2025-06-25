namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Collections.Generic;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using RoadRegistry.BackOffice.Core;
using TicketingService.Abstractions;

public class DeleteRoadSegmentOutlineSqsRequestHandler : SqsHandler<DeleteRoadSegmentOutlineSqsRequest>
{
    public const string Action = "DeleteRoadSegmentOutline";

    public DeleteRoadSegmentOutlineSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(DeleteRoadSegmentOutlineSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(request.Request.WegsegmentId));
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
