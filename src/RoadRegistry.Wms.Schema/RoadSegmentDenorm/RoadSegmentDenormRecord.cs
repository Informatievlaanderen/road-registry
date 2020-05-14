namespace RoadRegistry.Wms.Schema.RoadSegmentDenorm
{
    using System;
    using System.Data.SqlTypes;
    using System.Text.Json.Serialization;
    using Microsoft.Data.SqlClient.Server;
    using Microsoft.SqlServer.Types;
    using NetTopologySuite.Geometries;
    using Newtonsoft.Json;

    public class RoadSegmentDenormRecord
    {
        public int? Id { get; set; }
        public int? Method { get; set; }
        public string Maintainer { get; set; }
        public DateTime? BeginTime { get; set; }
        public string BeginOperator { get; set; }
        public string BeginOrganization { get; set; }

        public string BeginApplication { get; set; }

        public Geometry Geometry { get; set; }

        public int? Morphology { get; set; }

        public int? Status { get; set; }

        public string Category { get; set; }

        public int? BeginRoadNodeId { get; set; }

        public int? EndRoadNodeId { get; set; }

        public int? LeftSideStreetNameId { get; set; }

        public int? RightSideStreetNameId { get; set; }

        public int? RoadSegmentVersion { get; set; }

        public int? GeometryVersion { get; set; }

        public DateTime RecordingDate { get; set; }

        public int? AccessRestriction { get; set; }

        public int? TransactionId { get; set; }

        public int? SourceId { get; set; }

        public string SourceIdSource { get; set; }

        public int? LeftSideMunicipality { get; set; }

        public int? RightSideMunicipality { get; set; }

        public string CategoryLabel { get; set; }

        public string MethodLabel { get; set; }

        public string MorphologyLabel { get; set; }

        public string AccessRestrictionLabel { get; set; }

        public string StatusLabel { get; set; }

        public string OrganizationLabel { get; set; }

        public string LeftSideStreetNameLabel { get; set; }

        public string RightSideStreetNameLabel { get; set; }

        public string MaintainerLabel { get; set; }

        public Geometry Geometry2D { get; set; }

        public byte[] GeometryAsByte { get; set; }
    }

    public class RoadSegmentDenormTestRecord
    {
        public int Id { get; set; }

        public SqlBytes Geometrie { get; set; }
    }

}
