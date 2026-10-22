namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

// A road segment status change - 'markeer een gepland wegsegment als gerealiseerd', 'corrigeer een gerealiseerd
// wegsegment naar gepland', and every other transition in RoadSegmentStatusChange. Which transition it is, is the
// only thing that distinguishes one request from another: the request body is empty, the geometry is the one already
// on record, and everything the action does - which road nodes it snaps onto, which it has to add, which crossings
// become a gelijkgrondse kruising, which road nodes survive - follows from the network around the segment.
//
// One segment per request: a status change can alter the topology, so the order in which two of them are handled can
// affect the result.
[BlobRequest]
public sealed class ChangeRoadSegmentStatusV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    public required RoadSegmentStatusChange StatusChange { get; init; }

    // Which road segments the caller may modify, read off the scopes on their token at the edge - the only place they
    // are known - and enforced by the domain, which is the only place that knows which segments a change touches.
    public required EditableGeometryDrawMethods EditableGeometryDrawMethods { get; init; }
}
