namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

public sealed class UnlinkStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<UnlinkStreetNameRequest>
{
    public UnlinkStreetNameRequest Request { get; init; }
}
