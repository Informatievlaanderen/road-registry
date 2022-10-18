namespace RoadRegistry.Editor.Schema.Organizations;

public class OrganizationRecord
{
    public string Code { get; set; }
    public byte[] DbaseRecord { get; set; }
    public int Id { get; set; }
    public string SortableCode { get; set; }
}