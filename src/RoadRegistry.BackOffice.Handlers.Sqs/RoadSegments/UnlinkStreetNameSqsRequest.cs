namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class UnlinkStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<UnlinkStreetNameRequest>
{
    public UnlinkStreetNameRequest Request { get; init; }
}
