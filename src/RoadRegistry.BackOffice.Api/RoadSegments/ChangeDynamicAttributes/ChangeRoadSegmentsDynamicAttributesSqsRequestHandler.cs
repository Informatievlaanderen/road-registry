namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System.Collections.Generic;
using BackOffice.Handlers.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Core;
using TicketingService.Abstractions;

public class ChangeRoadSegmentsDynamicAttributesSqsRequestHandler : SqsHandler<ChangeRoadSegmentsDynamicAttributesSqsRequest>
{
    public const string Action = "ChangeRoadSegment";

    public ChangeRoadSegmentsDynamicAttributesSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeRoadSegmentsDynamicAttributesSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeRoadSegmentsDynamicAttributesSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
