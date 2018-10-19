namespace RoadRegistry.LegacyStreamExtraction
{
    using Aiv.Vbr.Shaperon;

    public class SpatialReferenceWriter
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly WellKnownBinaryWriter _wkbWriter;

        public SpatialReferenceWriter()
        {
            _wkbReader = new WellKnownBinaryReader();
            _wkbWriter = new WellKnownBinaryWriter();
        }

        public byte[] WriteWithSpatialReference(byte[] bytes)
        {
            var geometry = _wkbReader.Read(bytes);
            geometry.SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972;
            return _wkbWriter.Write(geometry);
        }
    }
}
