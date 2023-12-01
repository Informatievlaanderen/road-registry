namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class CorrectRoadNodeVersionsSqsRequestHandler : SqsHandler<CorrectRoadNodeVersionsSqsRequest>
{
    public const string Action = "CorrectRoadNodeVersions";

    public CorrectRoadNodeVersionsSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CorrectRoadNodeVersionsSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectRoadNodeVersionsSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
