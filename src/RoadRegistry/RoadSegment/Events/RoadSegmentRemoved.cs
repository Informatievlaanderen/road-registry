namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using ValueObjects;

public class RoadSegmentRemoved: IHaveHash
{
    public const string EventName = "RoadSegmentRemoved";

    public required RoadSegmentId Id { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
