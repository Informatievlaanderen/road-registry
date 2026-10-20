namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

[BlobRequest]
public sealed class SplitRoadSegmentSqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required Point CutPosition { get; init; }

    // Which road segments the caller may modify, read off the scopes on their token at the edge - the only place they
    // are known - and enforced by the domain, which is the only place that knows which segments a change touches.
    public required EditableGeometryDrawMethods EditableGeometryDrawMethods { get; init; }
}
