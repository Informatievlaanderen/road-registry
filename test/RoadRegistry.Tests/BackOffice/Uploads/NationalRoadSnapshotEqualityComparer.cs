namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

public class NationalRoadSnapshotEqualityComparer : IEqualityComparer<NationalRoadSnapshot>
{
    public bool Equals(NationalRoadSnapshot left, NationalRoadSnapshot right)
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
               && left.Number.Equals(right.Number)
               && left.IsRemoved.Equals(right.IsRemoved)
               && left.Origin.Equals(right.Origin)
               && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp);
    }

    public int GetHashCode(NationalRoadSnapshot obj)
    {
        throw new NotImplementedException();
    }
}
