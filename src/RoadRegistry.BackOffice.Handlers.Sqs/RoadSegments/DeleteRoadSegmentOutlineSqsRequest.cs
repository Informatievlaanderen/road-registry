namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using Abstractions.RoadSegmentsOutline;

public sealed class DeleteRoadSegmentOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<DeleteRoadSegmentOutlineRequest>
{
    public DeleteRoadSegmentOutlineRequest Request { get; init; }
}
