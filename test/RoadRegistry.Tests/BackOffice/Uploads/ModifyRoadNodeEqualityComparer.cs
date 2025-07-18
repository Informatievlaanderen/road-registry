namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class ModifyRoadNodeEqualityComparer : IEqualityComparer<ModifyRoadNode>
{
    private readonly bool _ignoreRecordNumber;

    public ModifyRoadNodeEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(ModifyRoadNode left, ModifyRoadNode right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        var sameGeometry =
            (left.Geometry == null && right.Geometry == null)
            || (left.Geometry != null && right.Geometry != null && left.Geometry.EqualsTopologically(right.Geometry));
        return left.Id.Equals(right.Id)
               && left.Type == right.Type
               && sameGeometry
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(ModifyRoadNode instance)
    {
        throw new NotSupportedException();
    }
}
