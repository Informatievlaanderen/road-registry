namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Dbase;
using TicketingService.Abstractions;

public class CorrectRoadSegmentVersionsSqsRequestHandler : SqsHandler<CorrectRoadSegmentVersionsSqsRequest>
{
    public const string Action = "CorrectRoadSegmentVersions";

    public CorrectRoadSegmentVersionsSqsRequestHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CorrectRoadSegmentVersionsSqsRequest request)
    {
        return RoadNetworkInfo.Identifier.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectRoadSegmentVersionsSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
