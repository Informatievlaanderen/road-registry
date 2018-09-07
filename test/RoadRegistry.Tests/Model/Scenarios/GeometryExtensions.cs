namespace RoadRegistry.Model
{
    using GeoAPI.Geometries;
    using Aiv.Vbr.Shaperon;

    internal static class GeometryExtensions
    {
        private static readonly WellKnownBinaryWriter Writer = new WellKnownBinaryWriter();

        public static byte[] ToBytes(this IGeometry geometry)
        {
            return Writer.Write(geometry);
        }
    }
}
