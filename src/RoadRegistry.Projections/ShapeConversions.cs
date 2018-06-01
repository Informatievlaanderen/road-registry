namespace RoadRegistry.Projections
{
    using Wkx;

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
            where TGeometry : Geometry
        {
            return (TGeometry)Geometry.Deserialize<WkbSerializer>(_value);
        }
    }
}
