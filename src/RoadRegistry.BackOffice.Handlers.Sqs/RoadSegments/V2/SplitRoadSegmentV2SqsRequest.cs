namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

[BlobRequest]
public sealed class SplitRoadSegmentV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadNodeGeometry CutPosition { get; init; }
}
