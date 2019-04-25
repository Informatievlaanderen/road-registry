namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeBoundingBox
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }

        public static RoadNodeBoundingBox From(PointM shape) => new RoadNodeBoundingBox
        {
            MinimumX = shape.EnvelopeInternal.MinX,
            MinimumY = shape.EnvelopeInternal.MinY,
            MaximumX = shape.EnvelopeInternal.MaxX,
            MaximumY = shape.EnvelopeInternal.MaxY
        };
    }
}
