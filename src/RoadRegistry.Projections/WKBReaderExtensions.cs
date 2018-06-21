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

        public static bool TryReadAs<TGeometry>(this WKBReader reader, byte[] value, out TGeometry geometry)
            where TGeometry : IGeometry
        {
            var parsed = reader.Read(value);
            if(parsed is TGeometry)
            {
                geometry = (TGeometry)parsed;
                return true;
            }
            geometry = default(TGeometry);
            return false;
        }
    }

}
