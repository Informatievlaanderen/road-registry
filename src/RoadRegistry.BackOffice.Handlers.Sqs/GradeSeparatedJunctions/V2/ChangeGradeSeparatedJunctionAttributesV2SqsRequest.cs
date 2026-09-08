namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// 'Wijzig attribuutwaarde(n) voor een ongelijkgrondse kruising': the two roads the wrong way round, or the wrong kind
// of crossing. Both are corrected in one request.
[BlobRequest]
public sealed class ChangeGradeSeparatedJunctionAttributesV2SqsRequest : SqsRequest
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }
}
