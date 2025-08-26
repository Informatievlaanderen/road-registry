namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class RequestExtractSqsRequestHandler : SqsHandler<RequestExtractSqsRequest>
{
    public const string Action = "RequestExtract";

    public RequestExtractSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(RequestExtractSqsRequest request)
    {
        return request.Request.ExtractRequestId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RequestExtractSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
