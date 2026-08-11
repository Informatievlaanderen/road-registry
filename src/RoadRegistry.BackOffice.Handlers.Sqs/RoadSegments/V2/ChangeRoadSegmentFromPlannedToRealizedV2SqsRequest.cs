namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;

// 'Markeer een gepland wegsegment als gerealiseerd'. The request body is empty: the geometry is the one already on
// record, and everything the action does - which road nodes it snaps onto, which it has to add, which crossings
// become a gelijkgrondse kruising - follows from the network around the segment.
//
// One segment per request: realizing changes the topology, so the order in which two of them are realized can affect
// the result.
[BlobRequest]
public sealed class ChangeRoadSegmentFromPlannedToRealizedV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    // VAL-9: whether the caller holds the 'ingemeten' scope, and may therefore realize a measured road segment.
    // Decided at the edge, where the caller's scopes are known, and enforced by the domain.
    public required bool MayModifyMeasuredRoadSegments { get; init; }
}
