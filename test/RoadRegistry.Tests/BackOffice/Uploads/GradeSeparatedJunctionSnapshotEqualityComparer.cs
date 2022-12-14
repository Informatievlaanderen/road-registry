namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

public class GradeSeparatedJunctionSnapshotEqualityComparer : IEqualityComparer<GradeSeparatedJunctionSnapshot>
{
    public bool Equals(GradeSeparatedJunctionSnapshot left, GradeSeparatedJunctionSnapshot right)
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
               && left.LowerRoadSegmentId.Equals(right.LowerRoadSegmentId)
               && left.UpperRoadSegmentId.Equals(right.UpperRoadSegmentId)
               && left.TypeId.Equals(right.TypeId)
               && left.TypeDutchName.Equals(right.TypeDutchName)
               && left.IsRemoved.Equals(right.IsRemoved)
               && left.Origin.Equals(right.Origin)
               && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp);
    }

    public int GetHashCode(GradeSeparatedJunctionSnapshot obj)
    {
        throw new NotImplementedException();
    }
}
