namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class DataValidationSqsRequestHandler : SqsHandler<DataValidationSqsRequest>
{
    public const string Action = "DataValidation";

    public DataValidationSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(DataValidationSqsRequest request)
    {
        return request.MigrateRoadNetworkSqsRequest.DownloadId.ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, DataValidationSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
