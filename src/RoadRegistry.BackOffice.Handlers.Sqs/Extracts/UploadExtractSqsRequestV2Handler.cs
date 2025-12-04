
namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class UploadExtractSqsRequestV2Handler : SqsHandler<UploadExtractSqsRequestV2>
{
    public const string Action = "UploadExtractV2";

    public UploadExtractSqsRequestV2Handler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(UploadExtractSqsRequestV2 request)
    {
        return request.ExtractRequestId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, UploadExtractSqsRequestV2 sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
