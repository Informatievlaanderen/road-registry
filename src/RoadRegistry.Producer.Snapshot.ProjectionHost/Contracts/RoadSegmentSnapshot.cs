namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using Common;
    using global::RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;
    using NetTopologySuite.Geometries;
    using Utilities.HexByteConvertor;

    public class RoadSegmentSnapshot : IQueueMessage
    {
        public string AccessRestrictionDutchName { get; }
        public int? AccessRestrictionId { get; }
        public int? BeginRoadNodeId { get; }
        public string CategoryDutchName { get; }
        public string CategoryId { get; }
        public int? EndRoadNodeId { get; }
        public string GeometryAsHex { get; }
        public string GeometryAsWkt { get; }
        public int GeometrySrid { get; }
        public int? GeometryVersion { get; }
        public int Id { get; }
        public int Version { get; }
        public int? LeftSideMunicipalityId { get; }
        public string LeftSideMunicipalityNisCode { get; }
        public string LeftSideStreetName { get; }
        public int? LeftSideStreetNameId { get; }
        public string MaintainerId { get; }
        public string MaintainerName { get; }
        public string MethodDutchName { get; }
        public int? MethodId { get; }
        public string MorphologyDutchName { get; }
        public int? MorphologyId { get; }
        public DateTime? RecordingDate { get; }
        public int? RightSideMunicipalityId { get; }
        public string RightSideMunicipalityNisCode { get; }
        public string RightSideStreetName { get; }
        public int? RightSideStreetNameId { get; }
        public int? RoadSegmentVersion { get; }
        public string StatusDutchName { get; }
        public int? StatusId { get; }
        public int? TransactionId { get; }
        
        public Origin Origin { get; }
        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public RoadSegmentSnapshot(
            string accessRestrictionDutchName,
            int? accessRestrictionId,
            int? beginRoadNodeId,
            string categoryDutchName,
            string categoryId,
            int? endRoadNodeId,
            Geometry geometry,
            int? geometryVersion,
            int id,
            int version,
            int? leftSideMunicipalityId,
            string leftSideMunicipalityNisCode,
            string leftSideStreetName,
            int? leftSideStreetNameId,
            string maintainerId,
            string maintainerName,
            string methodDutchName,
            int? methodId,
            string morphologyDutchName,
            int? morphologyId,
            DateTime? recordingDate,
            int? rightSideMunicipalityId,
            string rightSideMunicipalityNisCode,
            string rightSideStreetName,
            int? rightSideStreetNameId,
            int? roadSegmentVersion,
            string statusDutchName,
            int? statusId,
            int? transactionId,
            Origin origin,
            DateTimeOffset lastChangedTimestamp,
            bool isRemoved)
        {
            AccessRestrictionDutchName = accessRestrictionDutchName;
            AccessRestrictionId = accessRestrictionId;
            BeginRoadNodeId = beginRoadNodeId;
            CategoryDutchName = categoryDutchName;
            CategoryId = categoryId;
            EndRoadNodeId = endRoadNodeId;
            GeometryAsHex = geometry.ToBinary().ToHexString();
            GeometryAsWkt = geometry.AsText();
            GeometrySrid = geometry.SRID;
            GeometryVersion = geometryVersion;
            Id = id;
            Version = version;
            LeftSideMunicipalityId = leftSideMunicipalityId;
            LeftSideMunicipalityNisCode = leftSideMunicipalityNisCode;
            LeftSideStreetName = leftSideStreetName;
            LeftSideStreetNameId = leftSideStreetNameId;
            MaintainerId = maintainerId;
            MaintainerName = maintainerName;
            MethodDutchName = methodDutchName;
            MethodId = methodId;
            MorphologyDutchName = morphologyDutchName;
            MorphologyId = morphologyId;
            RecordingDate = recordingDate;
            RightSideMunicipalityId = rightSideMunicipalityId;
            RightSideMunicipalityNisCode = rightSideMunicipalityNisCode;
            RightSideStreetName = rightSideStreetName;
            RightSideStreetNameId = rightSideStreetNameId;
            RoadSegmentVersion = roadSegmentVersion;
            StatusDutchName = statusDutchName;
            StatusId = statusId;
            TransactionId = transactionId;

            Origin = origin;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
