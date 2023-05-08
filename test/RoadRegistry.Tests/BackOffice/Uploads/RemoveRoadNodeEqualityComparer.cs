namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveRoadNodeEqualityComparer : IEqualityComparer<RemoveRoadNode>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveRoadNodeEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveRoadNode left, RemoveRoadNode right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Id.Equals(right.Id)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(RemoveRoadNode instance)
    {
        throw new NotSupportedException();
    }
}
