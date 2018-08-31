namespace RoadRegistry.LegacyStreamExtraction
{
    using Shaperon;

    public class SpatialReferenceWriter
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly WellKnownBinaryWriter _wkbWriter;

        public SpatialReferenceWriter(SpatialReferenceSystemIdentifier spatialReferenceSystemIdentifier)
        {
            _wkbReader = new WellKnownBinaryReader(spatialReferenceSystemIdentifier);
            _wkbWriter = new WellKnownBinaryWriter();
        }

        public byte[] WriteWithSpatialReference(byte[] bytes)
        {
            return _wkbWriter.Write(_wkbReader.Read(bytes));
        }
    }
}
