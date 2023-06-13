namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public sealed class ChangeRoadSegmentsSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadSegmentsRequest>
{
    public ChangeRoadSegmentsRequest Request { get; init; }
}
