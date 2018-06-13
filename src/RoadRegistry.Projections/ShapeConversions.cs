namespace RoadRegistry.Projections
{
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    internal static class From
    {
        public static WellKnownBinaryGeometry WellKnownBinary(byte[] value)
        {
            return new WellKnownBinaryGeometry(value);
        }
    }

    public class WellKnownBinaryGeometry
    {
        private readonly byte[] _value;

        public WellKnownBinaryGeometry(byte[] value)
        {
            _value = value;
        }

        public TGeometry To<TGeometry>()
            where TGeometry : IGeometry
        {
            return (TGeometry)new WKBReader().Read(_value);
        }
    }
}
