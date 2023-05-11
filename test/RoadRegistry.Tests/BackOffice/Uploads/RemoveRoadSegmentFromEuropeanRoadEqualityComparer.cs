namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveRoadSegmentFromEuropeanRoadEqualityComparer : IEqualityComparer<RemoveRoadSegmentFromEuropeanRoad>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveRoadSegmentFromEuropeanRoadEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveRoadSegmentFromEuropeanRoad left, RemoveRoadSegmentFromEuropeanRoad right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.AttributeId.Equals(right.AttributeId)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(RemoveRoadSegmentFromEuropeanRoad instance)
    {
        throw new NotSupportedException();
    }
}
