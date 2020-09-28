namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class RemoveRoadSegmentEqualityComparer : IEqualityComparer<RemoveRoadSegment>
    {
        public bool Equals(RemoveRoadSegment left, RemoveRoadSegment right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Id.Equals(right.Id)
                   && left.RecordNumber.Equals(right.RecordNumber);
        }

        public int GetHashCode(RemoveRoadSegment instance)
        {
            throw new NotSupportedException();
        }
    }
}
