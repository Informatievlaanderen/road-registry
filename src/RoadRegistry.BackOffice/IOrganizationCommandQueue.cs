namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;
using Framework;

public interface IOrganizationCommandQueue
{
    Task Write(OrganizationId organizationId, Command command, CancellationToken cancellationToken);
}
