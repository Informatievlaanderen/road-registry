namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class CloseExtractSqsRequestHandler : SqsHandler<CloseExtractSqsRequest>
{
    public const string Action = "CloseExtract";

    public CloseExtractSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(CloseExtractSqsRequest request)
    {
        return new DownloadId(request.Request.DownloadId);
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CloseExtractSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
