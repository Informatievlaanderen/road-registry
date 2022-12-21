namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

public class RoadSegmentSurfaceSnapshotEqualityComparer : IEqualityComparer<RoadSegmentSurfaceSnapshot>
{
    public bool Equals(RoadSegmentSurfaceSnapshot left, RoadSegmentSurfaceSnapshot right)
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
               && left.RoadSegmentId.Equals(right.RoadSegmentId)
               && left.RoadSegmentGeometryVersion.Equals(right.RoadSegmentGeometryVersion)
               && left.TypeId.Equals(right.TypeId)
               && left.TypeDutchName == right.TypeDutchName
               && left.FromPosition.Equals(right.FromPosition)
               && left.ToPosition.Equals(right.ToPosition)
               && left.IsRemoved.Equals(right.IsRemoved)
               && left.Origin.Equals(right.Origin)
               && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp);
    }

    public int GetHashCode(RoadSegmentSurfaceSnapshot obj)
    {
        throw new NotImplementedException();
    }
}
