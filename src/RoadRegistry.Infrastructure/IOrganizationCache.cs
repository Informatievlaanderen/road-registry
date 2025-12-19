namespace RoadRegistry.Infrastructure;

using System.Threading;
using System.Threading.Tasks;
using BackOffice;

public interface IOrganizationCache
{
    Task<OrganizationDetail?> FindByIdOrOvoCodeOrKboNumberAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}

public class OrganizationDetail
{
    public OrganizationId Code { get; set; }
    public OrganizationName Name { get; set; }
    public OrganizationOvoCode? OvoCode { get; set; }
    public OrganizationKboNumber? KboNumber { get; set; }

    public static OrganizationDetail FromCode(OrganizationId code)
    {
        return new OrganizationDetail
        {
            Code = code,
            Name = new OrganizationName(code)
        };
    }
}
