namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

[BlobRequest]
public sealed class ChangeRoadSegmentOutlineGeometrySqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentOutlineGeometryRequest>
{
    public ChangeRoadSegmentOutlineGeometryRequest Request { get; init; }
}
