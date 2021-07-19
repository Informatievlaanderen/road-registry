namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    internal static class PolygonalGeometryTranslator
    {
        public static Polygon FromGeometry(NetTopologySuite.Geometries.IPolygonal polygonal)
        {
            switch (polygonal)
            {
                case NetTopologySuite.Geometries.MultiPolygon multiPolygon:
                    return GeometryTranslator.FromGeometryMultiPolygon(multiPolygon);
                case NetTopologySuite.Geometries.Polygon polygon:
                    return GeometryTranslator.FromGeometryPolygon(polygon);
                default:
                    throw new InvalidOperationException(
                        $"The polygonal was expected to be either a Polygon or MultiPolygon. The polygonal was {polygonal.GetType().Name}.");
            }
        }
    }
}
