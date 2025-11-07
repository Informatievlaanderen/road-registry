namespace RoadRegistry.RoadNode.Events;

using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadNetwork.ValueObjects;

public class RoadNodeRemoved: IHaveHash, ICreatedEvent
{
    public const string EventName = "RoadNodeRemoved";

    public required RoadNodeId Id { get; init; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
