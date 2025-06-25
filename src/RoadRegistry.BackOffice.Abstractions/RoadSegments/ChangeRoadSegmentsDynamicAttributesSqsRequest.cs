namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

[BlobRequest]
public sealed class ChangeRoadSegmentsDynamicAttributesSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentsDynamicAttributesRequest>
{
    public ChangeRoadSegmentsDynamicAttributesRequest Request { get; init; }
}
