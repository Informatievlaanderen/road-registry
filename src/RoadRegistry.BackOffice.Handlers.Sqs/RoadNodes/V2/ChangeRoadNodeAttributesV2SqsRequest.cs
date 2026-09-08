namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;

using System.Collections.Generic;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// Bulk 'wijzig attribuutwaarden' request for road nodes. The API layer has already validated the shape of the request,
// so what travels here is hard-typed: the nodes a change applies to and the value they should end up with.
[BlobRequest]
public sealed class ChangeRoadNodeAttributesV2SqsRequest : SqsRequest
{
    public required IReadOnlyList<ChangeRoadNodeAttributesV2Group> Groups { get; init; }
}

// One request object: the road nodes it applies to plus the attribute values they should take. 'Grensknoop' is not
// optional - a caller says what the value must become rather than leaving it to a default.
public sealed record ChangeRoadNodeAttributesV2Group
{
    public required IReadOnlyList<RoadNodeId> RoadNodeIds { get; init; }
    public required bool Grensknoop { get; init; }
}
