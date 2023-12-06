namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;

public interface IOrganizationRepository
{
    Task<OrganizationDetail?> FindByIdOrOvoCodeAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}
