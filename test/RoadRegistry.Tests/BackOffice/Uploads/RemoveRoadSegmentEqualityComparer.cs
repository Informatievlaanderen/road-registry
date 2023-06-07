namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveRoadSegmentEqualityComparer : IEqualityComparer<RemoveRoadSegment>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveRoadSegmentEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveRoadSegment left, RemoveRoadSegment right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Id.Equals(right.Id)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(RemoveRoadSegment instance)
    {
        throw new NotSupportedException();
    }
}