namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public sealed class ChangeRoadSegmentAttributesSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentAttributesRequest>
{
    public ChangeRoadSegmentAttributesRequest Request { get; init; }
}
