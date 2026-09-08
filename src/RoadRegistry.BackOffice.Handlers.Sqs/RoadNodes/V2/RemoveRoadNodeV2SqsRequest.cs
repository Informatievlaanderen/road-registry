namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.ValueObjects;

// 'Verwijder wegknoop', which is how road segments are merged: removing a road node that does not belong there is the
// same act as joining the roads that met at it. The node is the whole request - which roads become one follows from
// what hangs off it.
[BlobRequest]
public sealed class RemoveRoadNodeV2SqsRequest : SqsRequest
{
    public required RoadNodeId RoadNodeId { get; init; }
}
