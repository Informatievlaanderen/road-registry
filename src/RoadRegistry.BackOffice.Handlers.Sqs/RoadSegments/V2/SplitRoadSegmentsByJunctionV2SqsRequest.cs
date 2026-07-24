namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;

[BlobRequest]
public sealed class SplitRoadSegmentsByJunctionV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId1 { get; init; }
    public required RoadSegmentId RoadSegmentId2 { get; init; }
}
