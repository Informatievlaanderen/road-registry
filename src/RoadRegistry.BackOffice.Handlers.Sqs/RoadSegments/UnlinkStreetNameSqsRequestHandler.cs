namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using RoadSegment.ValueObjects;
using TicketingService.Abstractions;

public class UnlinkStreetNameSqsRequestHandler : SqsHandler<UnlinkStreetNameSqsRequest>
{
    public const string Action = "UnlinkStreetName";

    public UnlinkStreetNameSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(UnlinkStreetNameSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Get(new RoadSegmentId(request.Request.WegsegmentId), RoadSegmentGeometryDrawMethod.Parse(request.Request.Methode));
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, UnlinkStreetNameSqsRequest sqsRequest)
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
