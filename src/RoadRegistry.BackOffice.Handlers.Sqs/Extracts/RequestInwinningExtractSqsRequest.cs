namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.Extracts.V2;

public sealed class RequestInwinningExtractSqsRequest : SqsRequest, IHasBackOfficeRequest<RequestInwinningExtractRequest>
{
    public RequestInwinningExtractRequest Request { get; init; }
}
