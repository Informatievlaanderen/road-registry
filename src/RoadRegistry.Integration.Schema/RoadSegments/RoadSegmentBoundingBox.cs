namespace RoadRegistry.Integration.Schema.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;

public class RoadSegmentBoundingBox
{
    public double MaximumM { get; set; }
    public double MaximumX { get; set; }
    public double MaximumY { get; set; }
    public double MinimumM { get; set; }
    public double MinimumX { get; set; }
    public double MinimumY { get; set; }

    public static RoadSegmentBoundingBox From(PolyLineM shape)
    {
        var multiLineString = BackOffice.GeometryTranslator.ToMultiLineStringLambert72(shape);

        return new RoadSegmentBoundingBox
        {
            MinimumX = multiLineString.EnvelopeInternal.MinX,
            MinimumY = multiLineString.EnvelopeInternal.MinY,
            MaximumX = multiLineString.EnvelopeInternal.MaxX,
            MaximumY = multiLineString.EnvelopeInternal.MaxY,
            MinimumM = multiLineString.GetOrdinates(Ordinate.M).DefaultIfEmpty(double.NegativeInfinity).Min(),
            MaximumM = multiLineString.GetOrdinates(Ordinate.M).DefaultIfEmpty(double.PositiveInfinity).Max()
        };
    }
}
