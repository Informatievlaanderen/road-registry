namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

public class RoadNodeSnapshotEqualityComparer : IEqualityComparer<RoadNodeSnapshot>
{
    public bool Equals(RoadNodeSnapshot left, RoadNodeSnapshot right)
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
               && left.TypeId.Equals(right.TypeId)
               && left.TypeDutchName.Equals(right.TypeDutchName)
               && left.ExtendedWkbGeometryAsHex.Equals(right.ExtendedWkbGeometryAsHex)
               && left.WktGeometry.Equals(right.WktGeometry)
               && left.GeometrySrid.Equals(right.GeometrySrid)
               && left.Origin.Equals(right.Origin)
               && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp)
               && left.IsRemoved.Equals(right.IsRemoved);
    }

    public int GetHashCode(RoadNodeSnapshot obj)
    {
        throw new NotImplementedException();
    }
}
