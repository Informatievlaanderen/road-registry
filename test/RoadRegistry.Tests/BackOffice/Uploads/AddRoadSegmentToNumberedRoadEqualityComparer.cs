namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class AddRoadSegmentToNumberedRoadEqualityComparer : IEqualityComparer<AddRoadSegmentToNumberedRoad>
{
    public bool Equals(AddRoadSegmentToNumberedRoad left, AddRoadSegmentToNumberedRoad right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Number.Equals(right.Number)
               && left.SegmentId.Equals(right.SegmentId)
               && left.TemporaryAttributeId.Equals(right.TemporaryAttributeId)
               && left.Ordinal.Equals(right.Ordinal)
               && left.Direction.Equals(right.Direction)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(AddRoadSegmentToNumberedRoad instance)
    {
        throw new NotSupportedException();
    }
}