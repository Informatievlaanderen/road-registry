namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class ModifyRoadSegmentEqualityComparer : IEqualityComparer<ModifyRoadSegment>
{
    public bool Equals(ModifyRoadSegment left, ModifyRoadSegment right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        var sameGeometry =
            (left.Geometry == null && right.Geometry == null)
            || (left.Geometry != null && right.Geometry != null && left.Geometry.EqualsTopologically(right.Geometry));
        return left.Id.Equals(right.Id)
               && left.StartNodeId.Equals(right.StartNodeId)
               && left.EndNodeId.Equals(right.EndNodeId)
               && left.MaintenanceAuthority.Equals(right.MaintenanceAuthority)
               && left.Status.Equals(right.Status)
               && left.AccessRestriction.Equals(right.AccessRestriction)
               && left.Category.Equals(right.Category)
               && left.Morphology.Equals(right.Morphology)
               && left.GeometryDrawMethod.Equals(right.GeometryDrawMethod)
               && sameGeometry
               && left.LeftSideStreetNameId.Equals(right.LeftSideStreetNameId)
               && left.RightSideStreetNameId.Equals(right.RightSideStreetNameId)
               && left.Lanes.SequenceEqual(right.Lanes, new RoadSegmentLaneAttributeEqualityComparer())
               && left.Widths.SequenceEqual(right.Widths, new RoadSegmentWidthAttributeEqualityComparer())
               && left.Surfaces.SequenceEqual(right.Surfaces, new RoadSegmentSurfaceAttributeEqualityComparer())
               && left.RecordNumber.Equals(right.RecordNumber);
    }

    public int GetHashCode(ModifyRoadSegment instance)
    {
        throw new NotSupportedException();
    }
}