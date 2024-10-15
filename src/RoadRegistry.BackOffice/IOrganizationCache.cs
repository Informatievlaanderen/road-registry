namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;

public interface IOrganizationCache
{
    Task<OrganizationDetail> FindByIdOrOvoCodeOrKboNumberAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}
