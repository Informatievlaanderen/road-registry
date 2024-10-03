namespace RoadRegistry.Editor.Schema.Organizations;

public class OrganizationRecordV2
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string? OvoCode { get; set; }
    public string? KboNumber { get; set; }
    public bool IsMaintainer { get; set; }
}
