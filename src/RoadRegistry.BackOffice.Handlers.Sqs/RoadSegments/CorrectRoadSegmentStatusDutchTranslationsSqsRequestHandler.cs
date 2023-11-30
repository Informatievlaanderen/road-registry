namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class CorrectRoadSegmentStatusDutchTranslationsSqsRequestHandler : SqsHandler<CorrectRoadSegmentStatusDutchTranslationsSqsRequest>
{
    public const string Action = "RefreshRoadSegmentStatusDutchTranslations";

    public CorrectRoadSegmentStatusDutchTranslationsSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CorrectRoadSegmentStatusDutchTranslationsSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectRoadSegmentStatusDutchTranslationsSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
