namespace RoadRegistry.Wfs.Schema
{
    using System;
    using NetTopologySuite.Geometries;

    public class RoadSegmentRecord
    {
        public int Id { get; set; }
        public DateTime? BeginTime { get; set; }
        public Geometry Geometry2D { get; set; }
        public int? BeginRoadNodeId { get; set; }
        public int? EndRoadNodeId { get; set; }
        public string StatusDutchName { get; set; }
        public string MorphologyDutchName { get; set; }
        public string CategoryDutchName { get; set; }
        public int? LeftSideStreetNameId { get; set; }
        public string LeftSideStreetName { get; set; }
        public int? RightSideStreetNameId { get; set; }
        public string RightSideStreetName { get; set; }
        public int? AccessRestrictionId { get; set; }
        public string AccessRestriction { get; set; }
        public string MethodDutchName { get; set; }
        public string MaintainerId { get; set; }
        public string MaintainerName { get; set; }
        public long StreetNameCachePosition { get; set; }
    }
}
