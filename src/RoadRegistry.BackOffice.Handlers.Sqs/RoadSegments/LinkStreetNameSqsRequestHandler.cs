namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class LinkStreetNameSqsRequestHandler : SqsHandler<LinkStreetNameSqsRequest>
{
    public const string Action = "LinkStreetName";

    public LinkStreetNameSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(LinkStreetNameSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Get(new RoadSegmentId(request.Request.WegsegmentId), RoadSegmentGeometryDrawMethod.Parse(request.Request.Methode));
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, LinkStreetNameSqsRequest sqsRequest)
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
