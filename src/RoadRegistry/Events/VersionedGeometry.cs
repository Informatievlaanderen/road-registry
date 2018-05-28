namespace RoadRegistry.Events
{
    public class VersionedGeometry
    {
        public int Version { get; set; }
        public int SpatialReferenceSystemIdentifier { get; set; }
        public byte[] WellKnownBinary { get; set; }
    }
}
