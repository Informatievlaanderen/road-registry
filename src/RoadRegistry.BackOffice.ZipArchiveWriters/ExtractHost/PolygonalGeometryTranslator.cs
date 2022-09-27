namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using Polygon = Be.Vlaanderen.Basisregisters.Shaperon.Polygon;

internal static class PolygonalGeometryTranslator
{
    public static Polygon FromGeometry(IPolygonal polygonal)
    {
        switch (polygonal)
        {
            case MultiPolygon multiPolygon:
                return GeometryTranslator.FromGeometryMultiPolygon(multiPolygon);
            case NetTopologySuite.Geometries.Polygon polygon:
                return GeometryTranslator.FromGeometryPolygon(polygon);
            default:
                throw new InvalidOperationException(
                    $"The polygonal was expected to be either a Polygon or MultiPolygon. The polygonal was {polygonal.GetType().Name}.");
        }
    }
}
