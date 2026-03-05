namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed record RoadSegmentIdReference : IEquatable<RoadSegmentIdReference>
{
    public RoadSegmentId RoadSegmentId { get; init; }
    public IReadOnlyCollection<RoadSegmentTempId>? TempIds { get; init; }

    public RoadSegmentIdReference(RoadSegmentId roadSegmentId, IReadOnlyCollection<RoadSegmentTempId>? tempIds = null)
    {
        RoadSegmentId = roadSegmentId;
        TempIds = tempIds;
    }

    public bool Equals(RoadSegmentIdReference? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return RoadSegmentId.Equals(other.RoadSegmentId)
               && (TempIds ?? []).SequenceEqual(other.TempIds ?? []);
    }

    public string GetTempIdsAsString()
    {
        return string.Join(",", TempIds!.Select(x => x.ToInt32()));
    }
}
