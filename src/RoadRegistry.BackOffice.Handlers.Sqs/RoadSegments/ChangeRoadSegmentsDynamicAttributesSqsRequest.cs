namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public sealed class ChangeRoadSegmentsDynamicAttributesSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentsDynamicAttributesRequest>
{
    public ChangeRoadSegmentsDynamicAttributesRequest Request { get; init; }
}
