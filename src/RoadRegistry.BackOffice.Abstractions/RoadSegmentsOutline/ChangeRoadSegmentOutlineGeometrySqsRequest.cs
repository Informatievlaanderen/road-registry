namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

[BlobRequest]
public sealed class ChangeRoadSegmentOutlineGeometrySqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentOutlineGeometryRequest>
{
    public ChangeRoadSegmentOutlineGeometryRequest Request { get; init; }
}
