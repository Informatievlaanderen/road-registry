namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class ModifyRoadNodeEqualityComparer : IEqualityComparer<ModifyRoadNode>
    {
        public bool Equals(ModifyRoadNode left, ModifyRoadNode right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            var sameGeometry =
                left.Geometry == null && right.Geometry == null
                || left.Geometry != null && right.Geometry != null && left.Geometry.EqualsTopologically(right.Geometry);
            return left.Id.Equals(right.Id)
                   && left.Type.Equals(right.Type)
                   && sameGeometry
                   && left.RecordNumber.Equals(right.RecordNumber);
        }

        public int GetHashCode(ModifyRoadNode instance)
        {
            throw new NotSupportedException();
        }
    }
}
