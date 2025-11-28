namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.RoadNetwork.ValueObjects;

public interface IOrganizations
{
    Task<Organization> FindAsync(OrganizationId id, CancellationToken ct = default);
}
