namespace RoadRegistry.Projections
{
    using GeoAPI.Geometries;
    using NetTopologySuite.IO;

    public static class WKBReaderExtensions {
        public static TGeometry ReadAs<TGeometry>(this WKBReader reader, byte[] value)
            where TGeometry : IGeometry
        {
            return (TGeometry)reader.Read(value);
        }
    }

}
