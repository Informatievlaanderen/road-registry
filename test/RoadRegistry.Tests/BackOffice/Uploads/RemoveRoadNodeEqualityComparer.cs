namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

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
