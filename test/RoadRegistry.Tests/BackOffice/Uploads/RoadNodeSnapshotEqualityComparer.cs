namespace RoadRegistry.Tests.BackOffice.Uploads
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

    public class RoadNodeSnapshotEqualityComparer : IEqualityComparer<RoadNodeSnapshot>
    {
        public bool Equals(RoadNodeSnapshot left, RoadNodeSnapshot right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Id.Equals(right.Id)
                && left.Type.Equals(right.Type)
                && left.ExtendedWkbGeometryAsHex.Equals(right.ExtendedWkbGeometryAsHex)
                && left.WktGeometry.Equals(right.WktGeometry)
                && left.GeometrySrid.Equals(right.GeometrySrid)
                && left.IsRemoved.Equals(right.IsRemoved)
                && left.Organization.Equals(right.Organization)
                && left.BeginTime.Equals(right.BeginTime)
                && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp);
        }

        public int GetHashCode(RoadNodeSnapshot obj)
        {
            throw new NotImplementedException();
        }
    }
}
