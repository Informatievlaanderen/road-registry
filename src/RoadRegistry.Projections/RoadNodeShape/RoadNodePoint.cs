using Wkx;

namespace RoadRegistry.Projections
{
    public static class RoadNodePoint 
    {
        public static Point FromWellKnownBinary(byte[] value)
        {
            return (Point)Wkx.Geometry.Deserialize<WkbSerializer>(value);
        }
    }
}