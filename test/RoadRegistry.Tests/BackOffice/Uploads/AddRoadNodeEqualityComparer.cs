namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class AddRoadNodeEqualityComparer : IEqualityComparer<AddRoadNode>
{
    public bool Equals(AddRoadNode left, AddRoadNode right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        var sameGeometry =
            (left.Geometry == null && right.Geometry == null)
            || (left.Geometry != null && right.Geometry != null && left.Geometry.EqualsTopologically(right.Geometry));
        return left.TemporaryId.Equals(right.TemporaryId)
               && left.Type.Equals(right.Type)
               && sameGeometry
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(AddRoadNode instance)
    {
        throw new NotSupportedException();
    }
}