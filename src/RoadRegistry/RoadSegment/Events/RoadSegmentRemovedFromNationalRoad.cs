namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using ValueObjects;

public class RoadSegmentRemovedFromNationalRoad: IHaveHash
{
    public const string EventName = "RoadSegmentRemovedFromNationalRoad";

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
