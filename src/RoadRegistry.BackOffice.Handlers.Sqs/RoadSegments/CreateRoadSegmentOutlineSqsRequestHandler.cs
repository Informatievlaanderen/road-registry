namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Dbase;
using TicketingService.Abstractions;

public class CreateRoadSegmentOutlineSqsRequestHandler : SqsHandler<CreateRoadSegmentOutlineSqsRequest>
{
    public const string Action = "RoadSegmentOutline";

    public CreateRoadSegmentOutlineSqsRequestHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CreateRoadSegmentOutlineSqsRequest request)
    {
        return RoadNetworkInfo.Identifier.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateRoadSegmentOutlineSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
