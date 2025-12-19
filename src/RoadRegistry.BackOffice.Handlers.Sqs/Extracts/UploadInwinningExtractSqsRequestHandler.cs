
namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class UploadInwinningExtractSqsRequestV2Handler : SqsHandler<UploadInwinningExtractSqsRequest>
{
    public const string Action = "UploadInwinningExtract";

    public UploadInwinningExtractSqsRequestV2Handler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl)
        : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(UploadInwinningExtractSqsRequest request)
    {
        return request.ExtractRequestId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, UploadInwinningExtractSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
