namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveRoadSegmentFromNationalRoadEqualityComparer : IEqualityComparer<RemoveRoadSegmentFromNationalRoad>
{
    public bool Equals(RemoveRoadSegmentFromNationalRoad left, RemoveRoadSegmentFromNationalRoad right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.AttributeId.Equals(right.AttributeId)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(RemoveRoadSegmentFromNationalRoad instance)
    {
        throw new NotSupportedException();
    }
}
