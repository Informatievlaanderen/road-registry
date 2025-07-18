namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class ModifyRoadSegmentEqualityComparer : IEqualityComparer<ModifyRoadSegment>
{
    private readonly bool _ignoreRecordNumber;

    public ModifyRoadSegmentEqualityComparer(bool ignoreRecordNumber = false)
    {
        _ignoreRecordNumber = ignoreRecordNumber;
    }

    public bool Equals(ModifyRoadSegment left, ModifyRoadSegment right)
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
               && left.StartNodeId == right.StartNodeId
               && left.EndNodeId == right.EndNodeId
               && left.MaintenanceAuthority == right.MaintenanceAuthority
               && left.Status == right.Status
               && left.AccessRestriction == right.AccessRestriction
               && left.Category == right.Category
               && left.Morphology == right.Morphology
               && left.GeometryDrawMethod == right.GeometryDrawMethod
               && sameGeometry
               && left.LeftSideStreetNameId == right.LeftSideStreetNameId
               && left.RightSideStreetNameId == right.RightSideStreetNameId
               && (left.Lanes ?? []).SequenceEqual(right.Lanes ?? [], new RoadSegmentLaneAttributeEqualityComparer())
               && (left.Widths ?? []).SequenceEqual(right.Widths ?? [], new RoadSegmentWidthAttributeEqualityComparer())
               && (left.Surfaces ?? []).SequenceEqual(right.Surfaces ?? [], new RoadSegmentSurfaceAttributeEqualityComparer())
               && (_ignoreRecordNumber || left.RecordNumber.Equals(right.RecordNumber));
    }

    public int GetHashCode(ModifyRoadSegment instance)
    {
        throw new NotSupportedException();
    }
}
