namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;

    public class RoadSegmentLaneAttributeEqualityComparer : IEqualityComparer<RoadSegmentLaneAttribute>
    {
        public bool Equals(RoadSegmentLaneAttribute left, RoadSegmentLaneAttribute right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return true;
            return left.Direction.Equals(right.Direction)
                   && left.Count.Equals(right.Count)
                   && left.TemporaryId.Equals(right.TemporaryId)
                   && left.From.Equals(right.From)
                   && left.To.Equals(right.To);
        }

        public int GetHashCode(RoadSegmentLaneAttribute instance)
        {
            throw new NotSupportedException();
        }
    }
}