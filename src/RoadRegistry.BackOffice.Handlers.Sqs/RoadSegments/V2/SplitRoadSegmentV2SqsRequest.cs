namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

[BlobRequest]
public sealed class SplitRoadSegmentV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required Point CutPosition { get; init; }
}
