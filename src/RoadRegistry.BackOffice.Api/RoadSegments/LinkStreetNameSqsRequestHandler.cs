namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Collections.Generic;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Core;
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
