namespace RoadRegistry.GradeSeparatedJunction.Events;

using BackOffice;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadSegment.ValueObjects;

public class GradeSeparatedJunctionAdded : IHaveHash, ICreatedEvent
{
    public const string EventName = "GradeSeparatedJunctionAdded";

    public required GradeSeparatedJunctionId Id { get; init; }
    public required GradeSeparatedJunctionId TemporaryId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
