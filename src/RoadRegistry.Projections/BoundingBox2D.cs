namespace RoadRegistry.Projections
{
    using GeoAPI.Geometries;

    public class BoundingBox2D
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }

        public static BoundingBox2D From(Envelope envelope) => new BoundingBox2D
        {
            MinimumX = envelope.MinX,
            MinimumY = envelope.MinY,
            MaximumX = envelope.MaxX,
            MaximumY = envelope.MaxY
        };
    }
}
