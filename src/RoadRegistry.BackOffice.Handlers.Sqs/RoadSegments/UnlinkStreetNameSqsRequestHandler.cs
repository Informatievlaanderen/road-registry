namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Dbase;
using TicketingService.Abstractions;

public class UnlinkStreetNameSqsRequestHandler : SqsHandler<UnlinkStreetNameSqsRequest>
{
    public const string Action = "UnlinkStreetName";

    public UnlinkStreetNameSqsRequestHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(UnlinkStreetNameSqsRequest request)
    {
        return RoadNetworkInfo.Identifier.ToString();
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
