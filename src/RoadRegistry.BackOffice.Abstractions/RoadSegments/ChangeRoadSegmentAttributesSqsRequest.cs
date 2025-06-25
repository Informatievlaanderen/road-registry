namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

[BlobRequest]
public sealed class ChangeRoadSegmentAttributesSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentAttributesRequest>
{
    public ChangeRoadSegmentAttributesRequest Request { get; init; }
}
