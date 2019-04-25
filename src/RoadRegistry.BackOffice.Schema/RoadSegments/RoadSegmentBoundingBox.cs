namespace RoadRegistry.BackOffice.Schema.RoadSegments
{
    using System.Linq;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    public class RoadSegmentBoundingBox
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }
        public double MinimumM { get; set; }
        public double MaximumM { get; set; }

        public static RoadSegmentBoundingBox From(MultiLineString shape) => new RoadSegmentBoundingBox
        {
            MinimumX = shape.EnvelopeInternal.MinX,
            MinimumY = shape.EnvelopeInternal.MinY,
            MaximumX = shape.EnvelopeInternal.MaxX,
            MaximumY = shape.EnvelopeInternal.MaxY,
            MinimumM = shape.GetOrdinates(Ordinate.M).DefaultIfEmpty(double.NegativeInfinity).Min(),
            MaximumM = shape.GetOrdinates(Ordinate.M).DefaultIfEmpty(double.PositiveInfinity).Max()
        };
    }
}
