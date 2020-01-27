namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class RoadSegmentWidthAttributeEqualityComparer : IEqualityComparer<RoadSegmentWidthAttribute>
    {
        public bool Equals(RoadSegmentWidthAttribute left, RoadSegmentWidthAttribute right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return true;
            return left.Width.Equals(right.Width)
                   && left.TemporaryId.Equals(right.TemporaryId)
                   && left.From.Equals(right.From)
                   && left.To.Equals(right.To);
        }

        public int GetHashCode(RoadSegmentWidthAttribute instance)
        {
            throw new NotSupportedException();
        }
    }
}
