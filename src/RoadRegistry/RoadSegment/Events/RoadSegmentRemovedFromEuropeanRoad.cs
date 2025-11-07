namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using ValueObjects;

public class RoadSegmentRemovedFromEuropeanRoad: IHaveHash
{
    public const string EventName = "RoadSegmentRemovedFromEuropeanRoad";

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
