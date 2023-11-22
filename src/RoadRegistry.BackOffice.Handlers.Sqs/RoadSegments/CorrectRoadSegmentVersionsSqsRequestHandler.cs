namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class CorrectRoadSegmentVersionsSqsRequestHandler : SqsHandler<CorrectRoadSegmentVersionsSqsRequest>
{
    public const string Action = "CorrectRoadSegmentVersions";

    public CorrectRoadSegmentVersionsSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CorrectRoadSegmentVersionsSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default();
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
