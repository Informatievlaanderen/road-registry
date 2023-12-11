namespace RoadRegistry.BackOffice;

public class OrganizationDetail
{
    public OrganizationId Code { get; set; }
    public OrganizationName Name { get; set; }
    public OrganizationOvoCode? OvoCode { get; set; }

    public static OrganizationDetail FromCode(OrganizationId code)
    {
        return new OrganizationDetail
        {
            Code = code,
            Name = new OrganizationName(code)
        };
    }
}
