namespace RoadRegistry.Editor.Schema.RoadNodes;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

public class RoadNodeBoundingBox
{
    public static RoadNodeBoundingBox From(Point shape)
    {
        return new RoadNodeBoundingBox
        {
            MinimumX = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MinX,
            MinimumY = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MinY,
            MaximumX = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MaxX,
            MaximumY = GeometryTranslator.ToGeometryPoint(shape).EnvelopeInternal.MaxY
        };
    }

    public double MaximumX { get; set; }
    public double MaximumY { get; set; }
    public double MinimumX { get; set; }
    public double MinimumY { get; set; }

    public BoundingBox3D ToBoundingBox3D(double minimumM = default, double maximumM = default)
    {
        return new BoundingBox3D(
            MinimumX,
            MinimumY,
            MaximumX,
            MaximumY,
            0.0,
            0.0,
            minimumM,
            maximumM
        );
    }
}