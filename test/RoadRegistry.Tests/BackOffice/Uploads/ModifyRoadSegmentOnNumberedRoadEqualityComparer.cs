namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class ModifyRoadSegmentOnNumberedRoadEqualityComparer : IEqualityComparer<ModifyRoadSegmentOnNumberedRoad>
{
    public bool Equals(ModifyRoadSegmentOnNumberedRoad left, ModifyRoadSegmentOnNumberedRoad right)
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
               && left.Ordinal.Equals(right.Ordinal)
               && left.Direction.Equals(right.Direction);
    }

    public int GetHashCode(ModifyRoadSegmentOnNumberedRoad instance)
    {
        throw new NotSupportedException();
    }
}