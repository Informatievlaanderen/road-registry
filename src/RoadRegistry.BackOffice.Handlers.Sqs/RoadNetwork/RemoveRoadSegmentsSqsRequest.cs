namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public sealed class RemoveRoadSegmentsSqsRequest : SqsRequest
{
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; init; }
}
