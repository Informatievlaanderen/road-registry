namespace RoadRegistry.BackOffice.Messages
{
    public class MunicipalityGeometry
    {
        public int SpatialReferenceSystemIdentifier{ get; set; }
        public Ring[] Polygon { get; set; }
    }
}
