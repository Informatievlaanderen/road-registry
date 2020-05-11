namespace RoadRegistry.Editor.Schema.RoadNodes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    public class RoadNodeBoundingBox
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }

        public static RoadNodeBoundingBox From(Point shape) => new RoadNodeBoundingBox
        {
            MinimumX = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MinX,
            MinimumY = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MinY,
            MaximumX = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MaxX,
            MaximumY = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MaxY
        };
    }
}
