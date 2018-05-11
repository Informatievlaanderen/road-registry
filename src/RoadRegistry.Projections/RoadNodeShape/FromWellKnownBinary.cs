using Wkx;

namespace RoadRegistry.Projections
{
    public static class FromWellKnownBinary 
    {
        public static Point ToPoint(byte[] value)
        {
            return (Point)Wkx.Geometry.Deserialize<WkbSerializer>(value);
        }
    }
}