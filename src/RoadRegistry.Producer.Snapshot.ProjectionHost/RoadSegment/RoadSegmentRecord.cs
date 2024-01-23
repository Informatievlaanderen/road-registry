namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
    using NetTopologySuite.Geometries;
    using Schema;

    public class RoadSegmentRecord
    {
        public string AccessRestrictionDutchName { get; set; }
        public int? AccessRestrictionId { get; set; }
        public int? BeginRoadNodeId { get; set; }
        public string CategoryDutchName { get; set; }
        public string CategoryId { get; set; }
        public int? EndRoadNodeId { get; set; }
        public Geometry Geometry { get; set; }
        public int? GeometryVersion { get; set; }
        public int Id { get; set; }
        public int Version { get; set; }
        public int? LeftSideMunicipalityId { get; set; }
        public string LeftSideMunicipalityNisCode { get; set; }
        public string LeftSideStreetName { get; set; }
        public int? LeftSideStreetNameId { get; set; }
        public string MaintainerId { get; set; }
        public string MaintainerName { get; set; }
        public string MethodDutchName { get; set; }
        public int? MethodId { get; set; }
        public string MorphologyDutchName { get; set; }
        public int? MorphologyId { get; set; }
        public DateTime? RecordingDate { get; set; }
        public int? RightSideMunicipalityId { get; set; }
        public string RightSideMunicipalityNisCode { get; set; }
        public string RightSideStreetName { get; set; }
        public int? RightSideStreetNameId { get; set; }
        public int? RoadSegmentVersion { get; set; }
        public string StatusDutchName { get; set; }
        public int? StatusId { get; set; }
        public int? TransactionId { get; set; }
        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }
        public bool IsRemoved { get; set; }

        // EF needs this
        public RoadSegmentRecord() { }

        public RoadSegmentSnapshot ToContract()
        {
            return new RoadSegmentSnapshot(
                AccessRestrictionDutchName,
                AccessRestrictionId,
                BeginRoadNodeId,
                CategoryDutchName,
                CategoryId,
                EndRoadNodeId,
                Geometry,
                GeometryVersion,
                Id,
                Version,
                LeftSideMunicipalityId,
                LeftSideMunicipalityNisCode,
                LeftSideStreetName,
                LeftSideStreetNameId,
                MaintainerId,
                MaintainerName,
                MethodDutchName,
                MethodId,
                MorphologyDutchName,
                MorphologyId,
                RecordingDate,
                RightSideMunicipalityId,
                RightSideMunicipalityNisCode,
                RightSideStreetName,
                RightSideStreetNameId,
                RoadSegmentVersion,
                StatusDutchName,
                StatusId,
                TransactionId,
                Origin,
                LastChangedTimestamp,
                IsRemoved);
        }
    }
}
