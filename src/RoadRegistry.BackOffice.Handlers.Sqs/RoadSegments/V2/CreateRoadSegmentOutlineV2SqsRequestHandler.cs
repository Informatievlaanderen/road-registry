namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class CreateRoadSegmentOutlineV2SqsRequestHandler : SqsHandler<CreateRoadSegmentOutlineV2SqsRequest>
{
    public const string Action = "CreateRoadSegmentOutlineV2";

    public CreateRoadSegmentOutlineV2SqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CreateRoadSegmentOutlineV2SqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateRoadSegmentOutlineV2SqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
