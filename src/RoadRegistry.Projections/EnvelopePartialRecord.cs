namespace RoadRegistry.Projections
{
    using GeoAPI.Geometries;

    public class EnvelopePartialRecord
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }

        public static EnvelopePartialRecord From(Envelope envelope) => new EnvelopePartialRecord
        {
            MinimumX = envelope.MinX,
            MaximumX = envelope.MaxX,
            MinimumY = envelope.MinY,
            MaximumY = envelope.MaxY
        };

    }
}
