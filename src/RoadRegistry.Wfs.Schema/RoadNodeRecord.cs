namespace RoadRegistry.Wfs.Schema
{
    using System;
    using NetTopologySuite.Geometries;

    public class RoadNodeRecord
    {
        public int Id { get; set; }
        public DateTime? BeginTime { get; set; }
        public string Type { get; set; }
        public Geometry Geometry { get; set; }
    }
}
