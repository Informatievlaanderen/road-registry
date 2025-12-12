namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using CommandHandling.Actions.RemoveRoadSegments;

[BlobRequest]
public sealed class RemoveRoadSegmentsSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveRoadSegmentsCommand>
{
    public RemoveRoadSegmentsCommand Request { get; init; }
}
