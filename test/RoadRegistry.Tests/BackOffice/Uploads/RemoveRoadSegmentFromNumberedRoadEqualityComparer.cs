namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;

public class RemoveRoadSegmentFromNumberedRoadEqualityComparer : IEqualityComparer<RemoveRoadSegmentFromNumberedRoad>
{
    public bool Equals(RemoveRoadSegmentFromNumberedRoad left, RemoveRoadSegmentFromNumberedRoad right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.AttributeId.Equals(right.AttributeId)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(RemoveRoadSegmentFromNumberedRoad instance)
    {
        throw new NotSupportedException();
    }
}
