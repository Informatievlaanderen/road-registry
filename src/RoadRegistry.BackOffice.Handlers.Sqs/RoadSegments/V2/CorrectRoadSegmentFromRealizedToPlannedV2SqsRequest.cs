namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;

// 'Corrigeer een gerealiseerd wegsegment naar gepland'. The request body is empty: everything the action does -
// which road nodes survive, which crossings go - follows from the network around the segment.
//
// One segment per request: correcting one changes the topology, so the order in which two of them are corrected can
// affect the result.
[BlobRequest]
public sealed class CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest : SqsRequest
{
    public required RoadSegmentId RoadSegmentId { get; init; }

    // VAL-5: whether the caller holds the 'ingemeten' scope, and may therefore correct a measured road segment.
    // Decided at the edge, where the caller's scopes are known, and enforced by the domain.
    public required bool MayModifyMeasuredRoadSegments { get; init; }
}
