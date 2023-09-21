namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class CorrectRoadSegmentOrganizationNamesSqsRequestHandler : SqsHandler<CorrectRoadSegmentOrganizationNamesSqsRequest>
{
    public const string Action = "CorrectRoadSegmentOrganizationNames";

    public CorrectRoadSegmentOrganizationNamesSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CorrectRoadSegmentOrganizationNamesSqsRequest request)
    {
        return RoadNetwork.Identifier.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectRoadSegmentOrganizationNamesSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
