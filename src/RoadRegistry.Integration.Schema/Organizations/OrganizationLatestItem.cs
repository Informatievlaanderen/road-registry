namespace RoadRegistry.Integration.Schema.Organizations
{
    public class OrganizationLatestItem
    {
        public string Code { get; set; }
        public string SortableCode { get; set; }
        public string Name { get; set; }
        public string OvoCode { get; set; }
        public bool IsRemoved { get; set; }
    }
}
