namespace RoadRegistry.BackOffice.Messages
{
    public class ImportedMunicipality
    {
        public string NISCode { get; set; }
        public string DutchName { get; set; }
        public MunicipalityGeometry Geometry { get; set; }
        public string When { get; set; }
    }
}
