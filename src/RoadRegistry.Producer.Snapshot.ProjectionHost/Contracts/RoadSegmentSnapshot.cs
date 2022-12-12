namespace Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry
{
    using System;
    using NetTopologySuite.Geometries;
    using Utilities.HexByteConvertor;

    public class RoadSegmentSnapshot : IQueueMessage
    {

        public string AccessRestrictionDutchName { get; }
        public int? AccessRestrictionId { get; }
        public string BeginApplication { get; }
        public string BeginOperator { get; }
        public string BeginOrganizationId { get; }
        public string BeginOrganizationName { get; }
        public int? BeginRoadNodeId { get; }
        public DateTime? BeginTime { get; }
        public string CategoryDutchName { get; }
        public string CategoryId { get; }
        public int? EndRoadNodeId { get; }
        public string GeometryAsHex { get; }
        public string GeometryAsWkt { get; }
        public int GeometrySrid { get; }
        public int? GeometryVersion { get; }
        public int Id { get; }
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
        public long StreetNameCachePosition { get; }
        public int? TransactionId { get; }

        public DateTimeOffset LastChangedTimestamp { get; }
        public bool IsRemoved { get; }

        public RoadSegmentSnapshot(string accessRestrictionDutchName, int? accessRestrictionId, string beginApplication, string beginOperator, string beginOrganizationId, string beginOrganizationName, int? beginRoadNodeId, DateTime? beginTime, string categoryDutchName, string categoryId, int? endRoadNodeId, Geometry geometry2D, int? geometryVersion, int id, int? leftSideMunicipalityId, string leftSideMunicipalityNisCode, string leftSideStreetName, int? leftSideStreetNameId, string maintainerId, string maintainerName, string methodDutchName, int? methodId, string morphologyDutchName, int? morphologyId, DateTime? recordingDate, int? rightSideMunicipalityId, string rightSideMunicipalityNisCode, string rightSideStreetName, int? rightSideStreetNameId, int? roadSegmentVersion, string statusDutchName, int? statusId, long streetNameCachePosition, int? transactionId, DateTimeOffset lastChangedTimestamp, bool isRemoved)
        {
            AccessRestrictionDutchName = accessRestrictionDutchName;
            AccessRestrictionId = accessRestrictionId;
            BeginApplication = beginApplication;
            BeginOperator = beginOperator;
            BeginOrganizationId = beginOrganizationId;
            BeginOrganizationName = beginOrganizationName;
            BeginRoadNodeId = beginRoadNodeId;
            BeginTime = beginTime;
            CategoryDutchName = categoryDutchName;
            CategoryId = categoryId;
            EndRoadNodeId = endRoadNodeId;
            GeometryAsHex = geometry2D.ToBinary().ToHexString();
            GeometryAsWkt = geometry2D.AsText();
            GeometrySrid = geometry2D.SRID;
            GeometryVersion = geometryVersion;
            Id = id;
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
            StreetNameCachePosition = streetNameCachePosition;
            TransactionId = transactionId;
            LastChangedTimestamp = lastChangedTimestamp;
            IsRemoved = isRemoved;
        }
    }
}
