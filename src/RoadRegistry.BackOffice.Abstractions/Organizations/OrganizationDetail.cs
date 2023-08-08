namespace RoadRegistry.BackOffice.Abstractions.Organizations;

public class OrganizationDetail
{
    public OrganizationId Code { get; set; }
    public OrganizationName Name { get; set; }
    public OrganizationOvoCode? OvoCode { get; set; }
}
