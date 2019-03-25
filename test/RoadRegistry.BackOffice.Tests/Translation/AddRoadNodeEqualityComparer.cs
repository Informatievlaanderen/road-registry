namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;

    public class AddRoadNodeEqualityComparer : IEqualityComparer<AddRoadNode>
    {
        public bool Equals(AddRoadNode left, AddRoadNode right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.TemporaryId.Equals(right.TemporaryId)
                   && left.Type.Equals(right.Type)
                   && Equals(left.Geometry, right.Geometry)
                   && left.RecordNumber.Equals(right.RecordNumber);
        }

        public int GetHashCode(AddRoadNode instance)
        {
            throw new NotSupportedException();
        }
    }
}
