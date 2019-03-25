namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AddRoadSegmentEqualityComparer : IEqualityComparer<AddRoadSegment>
    {
        public bool Equals(AddRoadSegment left, AddRoadSegment right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.TemporaryId.Equals(right.TemporaryId)
                   && left.StartNodeId.Equals(right.StartNodeId)
                   && left.EndNodeId.Equals(right.EndNodeId)
                   && left.MaintenanceAuthority.Equals(right.MaintenanceAuthority)
                   && left.Status.Equals(right.Status)
                   && left.AccessRestriction.Equals(right.AccessRestriction)
                   && left.Category.Equals(right.Category)
                   && left.Morphology.Equals(right.Morphology)
                   && left.GeometryDrawMethod.Equals(right.GeometryDrawMethod)
                   && Equals(left.Geometry, right.Geometry)
                   && left.LeftSideStreetNameId.Equals(right.LeftSideStreetNameId)
                   && left.RightSideStreetNameId.Equals(right.RightSideStreetNameId)
                   && left.Lanes.SequenceEqual(right.Lanes, new RoadSegmentLaneAttributeEqualityComparer())
                   && left.Widths.SequenceEqual(right.Widths, new RoadSegmentWidthAttributeEqualityComparer())
                   && left.Surfaces.SequenceEqual(right.Surfaces, new RoadSegmentSurfaceAttributeEqualityComparer())
                   && left.RecordNumber.Equals(right.RecordNumber);
        }

        public int GetHashCode(AddRoadSegment instance)
        {
            throw new NotSupportedException();
        }
    }
}
