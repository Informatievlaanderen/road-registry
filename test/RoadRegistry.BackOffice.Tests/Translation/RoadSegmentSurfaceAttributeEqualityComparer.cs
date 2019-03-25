namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;

    public class RoadSegmentSurfaceAttributeEqualityComparer : IEqualityComparer<RoadSegmentSurfaceAttribute>
    {
        public bool Equals(RoadSegmentSurfaceAttribute left, RoadSegmentSurfaceAttribute right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return true;
            return left.Type.Equals(right.Type)
                   && left.TemporaryId.Equals(right.TemporaryId)
                   && left.From.Equals(right.From)
                   && left.To.Equals(right.To);
        }

        public int GetHashCode(RoadSegmentSurfaceAttribute instance)
        {
            throw new NotSupportedException();
        }
    }
}
