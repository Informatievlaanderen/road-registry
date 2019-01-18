namespace RoadRegistry.BackOfficeSchema
{
    using Aiv.Vbr.Shaperon;
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

        public BoundingBox3D ToBoundingBox3D(double minimumM = default, double maximumM = default) => new BoundingBox3D(
            MinimumX,
            MaximumX,
            MinimumY,
            MaximumY,
            0.0,
            0.0,
            minimumM,
            maximumM
        );
    }
}
