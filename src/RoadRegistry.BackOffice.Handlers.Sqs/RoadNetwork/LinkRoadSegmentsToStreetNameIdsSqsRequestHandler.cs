namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class LinkRoadSegmentsToStreetNameIdsSqsRequestHandler : SqsHandler<LinkRoadSegmentsToStreetNameIdsSqsRequest>
{
    public const string Action = "LinkRoadSegmentsToStreetNameIds";

    public LinkRoadSegmentsToStreetNameIdsSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(LinkRoadSegmentsToStreetNameIdsSqsRequest request)
    {
        return Constants.GlobalMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, LinkRoadSegmentsToStreetNameIdsSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
