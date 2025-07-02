namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

public sealed class LinkStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<LinkStreetNameRequest>
{
    public LinkStreetNameRequest Request { get; init; }
}
