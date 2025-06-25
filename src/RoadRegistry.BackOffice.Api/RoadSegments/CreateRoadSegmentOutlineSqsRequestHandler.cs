namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Collections.Generic;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using TicketingService.Abstractions;

public class CreateRoadSegmentOutlineSqsRequestHandler : SqsHandler<CreateRoadSegmentOutlineSqsRequest>
{
    public const string Action = "CreateRoadSegmentOutline";

    public CreateRoadSegmentOutlineSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CreateRoadSegmentOutlineSqsRequest request)
    {
        return Guid.NewGuid().ToString();
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
