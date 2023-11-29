namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveOutlinedRoadSegmentEqualityComparer : IEqualityComparer<RemoveOutlinedRoadSegment>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveOutlinedRoadSegmentEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveOutlinedRoadSegment left, RemoveOutlinedRoadSegment right)
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

    public int GetHashCode(RemoveOutlinedRoadSegment instance)
    {
        throw new NotSupportedException();
    }
}
