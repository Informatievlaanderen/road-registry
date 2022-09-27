namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class AddGradeSeparatedJunctionEqualityComparer : IEqualityComparer<AddGradeSeparatedJunction>
{
    public bool Equals(AddGradeSeparatedJunction left, AddGradeSeparatedJunction right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.TemporaryId.Equals(right.TemporaryId)
               && left.Type.Equals(right.Type)
               && left.UpperSegmentId.Equals(right.UpperSegmentId)
               && left.LowerSegmentId.Equals(right.LowerSegmentId)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(AddGradeSeparatedJunction instance)
    {
        throw new NotSupportedException();
    }
}
