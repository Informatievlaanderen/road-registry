namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class LinkStreetNameSqsRequest : SqsRequest, IHasBackOfficeRequest<LinkStreetNameRequest>
{
    public LinkStreetNameRequest Request { get; init; }
}
