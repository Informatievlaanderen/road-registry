namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Schema
{
    using System;
    using NetTopologySuite.Geometries;

    public class RoadNodeRecord
    {
        public int Id { get; set; }

        public string Type { get; set; }
        public Geometry Geometry { get; set; }

        public Origin Origin { get; set; }
        public DateTimeOffset LastChangedTimestamp { get; set; }

        // EF needs this
        private RoadNodeRecord() { }

        public RoadNodeRecord(
            int id,
            string type,
            Point point,
            DateTime? beginTime,
            string organization,
            DateTimeOffset lastChangedTimestamp)
        {
            Id = id;
            Origin = new Origin
            {
                Organization = organization,
                BeginTime = beginTime
            };
            Type = type;
            Geometry = point;
            LastChangedTimestamp = lastChangedTimestamp;
        }
    }

    //public class RoadNodeBoundingBox
    //{
    //    public double MaximumX { get; set; }
    //    public double MaximumY { get; set; }
    //    public double MinimumX { get; set; }
    //    public double MinimumY { get; set; }

    //    public static RoadNodeBoundingBox From(NetTopologySuite.Geometries.Point point)
    //    {
    //        return new RoadNodeBoundingBox
    //        {
    //            MinimumX = point.EnvelopeInternal.MinX,
    //            MinimumY = point.EnvelopeInternal.MinY,
    //            MaximumX = point.EnvelopeInternal.MaxX,
    //            MaximumY = point.EnvelopeInternal.MaxY
    //        };
    //    }
    //}
}
