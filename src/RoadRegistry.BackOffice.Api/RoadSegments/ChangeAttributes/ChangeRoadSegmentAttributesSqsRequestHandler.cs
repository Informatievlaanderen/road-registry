namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System;
using System.Collections.Generic;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using TicketingService.Abstractions;

public class ChangeRoadSegmentAttributesSqsRequestHandler : SqsHandler<ChangeRoadSegmentAttributesSqsRequest>
{
    public const string Action = "ChangeRoadSegmentAttributes";

    public ChangeRoadSegmentAttributesSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentAttributesSqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentAttributesSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
