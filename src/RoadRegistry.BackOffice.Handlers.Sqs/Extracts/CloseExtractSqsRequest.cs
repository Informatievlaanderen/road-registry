namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.Extracts.V2;

public sealed class CloseExtractSqsRequest : SqsRequest, IHasBackOfficeRequest<CloseExtractRequest>
{
    public CloseExtractRequest Request { get; init; }
}
