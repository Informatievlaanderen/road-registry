namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveRoadSegmentFromNumberedRoadEqualityComparer : IEqualityComparer<RemoveRoadSegmentFromNumberedRoad>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveRoadSegmentFromNumberedRoadEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveRoadSegmentFromNumberedRoad left, RemoveRoadSegmentFromNumberedRoad right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.AttributeId.Equals(right.AttributeId)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(RemoveRoadSegmentFromNumberedRoad instance)
    {
        throw new NotSupportedException();
    }
}