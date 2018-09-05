namespace Shaperon
{
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public class WellKnownBinaryReader
    {
        private readonly WKBReader _wkbReader;

        public WellKnownBinaryReader()
        {
            _wkbReader = new WKBReader(GeometryConfiguration.GeometryFactory)
            {
                HandleOrdinates = Ordinates.XYZM,
                HandleSRID = true
            };
        }

        public IGeometry Read(byte[] data)
        {
            var geometry = _wkbReader.Read(data);
            if (geometry is Point point)
                return new MeasuredPoint(point.X, point.Y, point.Z, point.M);

            return geometry;
        }

        public TGeometry ReadAs<TGeometry>(byte[] value)
            where TGeometry : IGeometry
        {
            return (TGeometry)Read(value);
        }

        public bool TryReadAs<TGeometry>(byte[] value, out TGeometry geometry)
            where TGeometry : IGeometry
        {
            var parsed = Read(value);
            if (parsed is TGeometry parsedGeometry)
            {
                geometry = parsedGeometry;
                return true;
            }
            geometry = default(TGeometry);
            return false;
        }
    }
}
