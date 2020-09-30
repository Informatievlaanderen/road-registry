namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class ModifyGradeSeparatedJunctionEqualityComparer : IEqualityComparer<ModifyGradeSeparatedJunction>
    {
        public bool Equals(ModifyGradeSeparatedJunction left, ModifyGradeSeparatedJunction right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Id.Equals(right.Id)
                   && left.Type.Equals(right.Type)
                   && left.UpperSegmentId.Equals(right.UpperSegmentId)
                   && left.LowerSegmentId.Equals(right.LowerSegmentId)
                   && left.RecordNumber.Equals(right.RecordNumber);
        }

        public int GetHashCode(ModifyGradeSeparatedJunction instance)
        {
            throw new NotSupportedException();
        }
    }
}
