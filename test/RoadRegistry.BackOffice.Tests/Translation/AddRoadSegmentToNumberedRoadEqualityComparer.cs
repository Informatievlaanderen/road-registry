namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;

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
                   && left.Direction.Equals(right.Direction);
        }

        public int GetHashCode(AddRoadSegmentToNumberedRoad instance)
        {
            throw new NotSupportedException();
        }
    }
}