namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

[BlobRequest]
public sealed class CreateRoadSegmentOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<CreateRoadSegmentOutlineRequest>
{
    public CreateRoadSegmentOutlineRequest Request { get; init; }
}
