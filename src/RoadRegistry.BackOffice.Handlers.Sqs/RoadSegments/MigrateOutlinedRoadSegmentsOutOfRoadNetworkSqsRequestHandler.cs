namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using Core;
using TicketingService.Abstractions;

public class MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequestHandler : SqsHandler<MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest>
{
    public const string Action = "MigrateOutlinedRoadSegmentsOutOfRoadNetwork";

    public MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequestHandler(IAdminSqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest request)
    {
        return RoadNetworkStreamNameProvider.Default;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
