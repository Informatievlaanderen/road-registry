namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;

public class RemoveRoadNodeEqualityComparer : IEqualityComparer<RemoveRoadNode>
{
    public bool Equals(RemoveRoadNode left, RemoveRoadNode right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Id.Equals(right.Id)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(RemoveRoadNode instance)
    {
        throw new NotSupportedException();
    }
}
