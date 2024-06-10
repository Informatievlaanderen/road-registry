namespace RoadRegistry.Integration.Schema.RoadSegments;

using System.Collections.Generic;

public class RoadSegmentRecordEqualityComparerById : IEqualityComparer<RoadSegmentLatestItem>
{
    public bool Equals(RoadSegmentLatestItem x, RoadSegmentLatestItem y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(RoadSegmentLatestItem obj)
    {
        return obj.Id;
    }
}