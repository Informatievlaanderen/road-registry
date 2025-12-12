namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class RequestInwinningExtractSqsRequestHandler : SqsHandler<RequestInwinningExtractSqsRequest>
{
    public const string Action = "RequestInwinningExtract";

    public RequestInwinningExtractSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(RequestInwinningExtractSqsRequest request)
    {
        return request.Request.ExtractRequestId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RequestInwinningExtractSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
