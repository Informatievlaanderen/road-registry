namespace RoadRegistry.Tests.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;

public class RoadSegmentSnapshotEqualityComparer : IEqualityComparer<RoadSegmentSnapshot>
{
    public bool Equals(RoadSegmentSnapshot left, RoadSegmentSnapshot right)
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
               && left.MaintainerId == right.MaintainerId
               && left.MaintainerName == right.MaintainerName
               && left.MethodId.Equals(right.MethodId)
               && left.MethodDutchName == right.MethodDutchName
               && left.CategoryId.Equals(right.CategoryId)
               && left.CategoryDutchName == right.CategoryDutchName
               && left.GeometryAsHex.Equals(right.GeometryAsHex)
               && left.GeometryAsWkt.Equals(right.GeometryAsWkt)
               && left.GeometrySrid.Equals(right.GeometrySrid)
               && left.GeometryVersion.Equals(right.GeometryVersion)
               && left.MorphologyId.Equals(right.MorphologyId)
               && left.MorphologyDutchName == right.MorphologyDutchName
               && left.StatusId.Equals(right.StatusId)
               && left.StatusDutchName == right.StatusDutchName
               && left.AccessRestrictionId.Equals(right.AccessRestrictionId)
               && left.AccessRestrictionDutchName == right.AccessRestrictionDutchName
               && left.RecordingDate.Equals(right.RecordingDate)
               && left.TransactionId.Equals(right.TransactionId)
               && left.LeftSideMunicipalityId.Equals(right.LeftSideMunicipalityId)
               && left.LeftSideMunicipalityNisCode == right.LeftSideMunicipalityNisCode
               && left.LeftSideStreetNameId.Equals(right.LeftSideStreetNameId)
               && left.LeftSideStreetName == right.LeftSideStreetName
               && left.RightSideMunicipalityId.Equals(right.RightSideMunicipalityId)
               && left.RightSideMunicipalityNisCode == right.RightSideMunicipalityNisCode
               && left.RightSideStreetNameId.Equals(right.RightSideStreetNameId)
               && left.RightSideStreetName == right.RightSideStreetName
               && left.RoadSegmentVersion.Equals(right.RoadSegmentVersion)
               && left.BeginRoadNodeId.Equals(right.BeginRoadNodeId)
               && left.EndRoadNodeId.Equals(right.EndRoadNodeId)
               && left.Origin.Equals(right.Origin)
               && left.LastChangedTimestamp.Equals(right.LastChangedTimestamp)
               && left.IsRemoved.Equals(right.IsRemoved);
    }

    public int GetHashCode(RoadSegmentSnapshot obj)
    {
        throw new NotImplementedException();
    }
}
