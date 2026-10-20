namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

[BlobRequest]
public sealed class SplitRoadSegmentsByJunctionSqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId1 { get; init; }
    public required RoadSegmentId RoadSegmentId2 { get; init; }

    // Which road segments the caller may modify, read off the scopes on their token at the edge - the only place they
    // are known - and enforced by the domain, which is the only place that knows which segments a change touches.
    public required EditableGeometryDrawMethods EditableGeometryDrawMethods { get; init; }
}
