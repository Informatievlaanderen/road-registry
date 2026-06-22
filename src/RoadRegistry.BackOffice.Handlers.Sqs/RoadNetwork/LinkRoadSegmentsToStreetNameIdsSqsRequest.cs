namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using System.Collections.Generic;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

[BlobRequest]
public sealed class LinkRoadSegmentsToStreetNameIdsSqsRequest : SqsRequest
{
    public required IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; init; }
    public required StreetNameLocalId StreetNameId { get; init; }
}
