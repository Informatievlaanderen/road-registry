namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

[BlobRequest]
public sealed class CreateRoadSegmentOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<CreateRoadSegmentOutlineRequest>
{
    public CreateRoadSegmentOutlineRequest Request { get; init; }
}
