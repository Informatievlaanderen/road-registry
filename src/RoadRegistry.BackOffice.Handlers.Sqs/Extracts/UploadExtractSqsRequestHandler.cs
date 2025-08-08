
namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class UploadExtractSqsRequestHandler : SqsHandler<UploadExtractSqsRequest>
{
    public const string Action = "UploadExtract";

    public UploadExtractSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(UploadExtractSqsRequest request)
    {
        return request.ExtractRequestId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, UploadExtractSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
