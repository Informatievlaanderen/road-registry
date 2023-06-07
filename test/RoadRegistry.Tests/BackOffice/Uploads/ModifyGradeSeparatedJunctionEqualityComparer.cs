namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class ModifyGradeSeparatedJunctionEqualityComparer : IEqualityComparer<ModifyGradeSeparatedJunction>
{
    private readonly bool _ignoreRecordNumber;

    public ModifyGradeSeparatedJunctionEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(ModifyGradeSeparatedJunction left, ModifyGradeSeparatedJunction right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Id.Equals(right.Id)
               && left.Type.Equals(right.Type)
               && left.UpperSegmentId.Equals(right.UpperSegmentId)
               && left.LowerSegmentId.Equals(right.LowerSegmentId)
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(ModifyGradeSeparatedJunction instance)
    {
        throw new NotSupportedException();
    }
}