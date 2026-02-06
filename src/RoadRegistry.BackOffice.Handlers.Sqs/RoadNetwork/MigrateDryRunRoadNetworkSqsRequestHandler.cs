namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class MigrateDryRunRoadNetworkSqsRequestHandler : SqsHandler<MigrateDryRunRoadNetworkSqsRequest>
{
    public const string Action = "MigrateDryRunRoadNetwork";

    public MigrateDryRunRoadNetworkSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(MigrateDryRunRoadNetworkSqsRequest request)
    {
        return request.MigrateRoadNetworkSqsRequest.DownloadId.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, MigrateDryRunRoadNetworkSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
