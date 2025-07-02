namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;

public sealed class DeleteRoadSegmentOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<DeleteRoadSegmentOutlineRequest>
{
    public DeleteRoadSegmentOutlineRequest Request { get; init; }
}
