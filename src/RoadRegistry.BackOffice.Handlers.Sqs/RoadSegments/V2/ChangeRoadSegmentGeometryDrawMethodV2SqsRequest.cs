namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using System.Collections.Generic;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// Bulk 'wijzig geometriemethode' request. The API layer already parsed the Dutch value into its value object
// (validation = "does it parse"), so the request carries hard types.
[BlobRequest]
public sealed class ChangeRoadSegmentGeometryDrawMethodV2SqsRequest : SqsRequest
{
    public required IReadOnlyList<ChangeRoadSegmentGeometryDrawMethodV2Group> Groups { get; init; }
}

// One request object: the segments it applies to plus the draw method they change to.
public sealed record ChangeRoadSegmentGeometryDrawMethodV2Group
{
    public required IReadOnlyList<RoadSegmentId> RoadSegmentIds { get; init; }
    public required RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }
}
