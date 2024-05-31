namespace RoadRegistry.Integration.Schema.RoadSegments;

using System.Collections.Generic;

public class RoadSegmentRecordEqualityComparerById : IEqualityComparer<RoadSegmentRecord>
{
    public bool Equals(RoadSegmentRecord x, RoadSegmentRecord y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(RoadSegmentRecord obj)
    {
        return obj.Id;
    }
}