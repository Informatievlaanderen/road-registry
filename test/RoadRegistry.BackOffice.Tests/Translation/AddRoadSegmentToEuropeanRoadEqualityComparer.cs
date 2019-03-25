namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;

    public class AddRoadSegmentToEuropeanRoadEqualityComparer : IEqualityComparer<AddRoadSegmentToEuropeanRoad>
    {
        public bool Equals(AddRoadSegmentToEuropeanRoad left, AddRoadSegmentToEuropeanRoad right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Number.Equals(right.Number)
                   && left.SegmentId.Equals(right.SegmentId)
                   && left.TemporaryAttributeId.Equals(right.TemporaryAttributeId);
        }

        public int GetHashCode(AddRoadSegmentToEuropeanRoad instance)
        {
            throw new NotSupportedException();
        }
    }
}