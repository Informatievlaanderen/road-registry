namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

public sealed class CreateRoadSegmentOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<CreateRoadSegmentOutlineRequest>
{
    public CreateRoadSegmentOutlineRequest Request { get; init; }
}
