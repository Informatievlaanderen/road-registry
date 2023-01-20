namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class CorrectRoadSegmentVersionsSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRoadSegmentVersionsRequest>
{
    public CorrectRoadSegmentVersionsRequest Request { get; init; }
}
