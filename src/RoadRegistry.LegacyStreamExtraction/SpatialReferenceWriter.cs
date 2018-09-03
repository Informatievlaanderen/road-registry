namespace RoadRegistry.LegacyStreamExtraction
{
    using Shaperon;

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
            return _wkbWriter.Write(_wkbReader.Read(bytes));
        }
    }
}
