namespace RoadRegistry.RoadNode.Events;

using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadNetwork.ValueObjects;

public class RoadNodeModified: IHaveHash, ICreatedEvent
{
    public const string EventName = "RoadNodeModified";

    public required RoadNodeId Id { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeType Type { get; init; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
