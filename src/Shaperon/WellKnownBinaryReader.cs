namespace Shaperon
{
    using GeoAPI.Geometries;
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
            return _wkbReader.Read(data);
        }

        public TGeometry ReadAs<TGeometry>(byte[] value)
            where TGeometry : IGeometry
        {
            return (TGeometry)_wkbReader.Read(value);
        }

        public bool TryReadAs<TGeometry>(byte[] value, out TGeometry geometry)
            where TGeometry : IGeometry
        {
            var parsed = _wkbReader.Read(value);
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
