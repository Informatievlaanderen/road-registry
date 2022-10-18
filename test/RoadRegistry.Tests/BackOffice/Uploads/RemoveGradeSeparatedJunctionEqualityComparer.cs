namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveGradeSeparatedJunctionEqualityComparer : IEqualityComparer<RemoveGradeSeparatedJunction>
{
    public bool Equals(RemoveGradeSeparatedJunction left, RemoveGradeSeparatedJunction right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Id.Equals(right.Id)
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(RemoveGradeSeparatedJunction instance)
    {
        throw new NotSupportedException();
    }
}