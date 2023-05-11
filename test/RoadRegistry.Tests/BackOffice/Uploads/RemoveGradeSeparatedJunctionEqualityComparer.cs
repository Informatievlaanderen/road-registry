namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class RemoveGradeSeparatedJunctionEqualityComparer : IEqualityComparer<RemoveGradeSeparatedJunction>
{
    private readonly bool _ignoreRecordNumber;

    public RemoveGradeSeparatedJunctionEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(RemoveGradeSeparatedJunction left, RemoveGradeSeparatedJunction right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        return left.Id.Equals(right.Id)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(RemoveGradeSeparatedJunction instance)
    {
        throw new NotSupportedException();
    }
}
