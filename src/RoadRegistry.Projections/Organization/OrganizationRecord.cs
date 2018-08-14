namespace RoadRegistry.Projections
{
    public class OrganizationRecord
    {
        public int Id { get; set; }
        public byte[] DbaseRecord { get; set; }
        public string Code { get; set; }
        public string SortCode { get; set; }
    }
}
