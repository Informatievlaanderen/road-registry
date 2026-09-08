namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeJunctions.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// 'Wijzig gelijkgrondse kruising naar ongelijkgrondse kruising'. Realizing a road segment records a grade junction
// wherever it crosses another road, but a crossing can turn out to be a bridge or a tunnel. Which of the two roads
// goes under and which goes over cannot be derived from the geometry, so the caller names both.
[BlobRequest]
public sealed class ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest : SqsRequest
{
    public required GradeJunctionId GradeJunctionId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }
}
