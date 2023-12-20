namespace RoadRegistry.SyncHost.Tests.Organization;

using Sync.OrganizationRegistry;
using Sync.OrganizationRegistry.Models;

public class FakeOrganizationReader : IOrganizationReader
{
    private readonly Organization[] _organizations;

    public FakeOrganizationReader(params Organization[] organizations)
    {
        _organizations = organizations;
    }

    public async Task ReadAsync(long startAtChangeId, Func<Organization, Task> handler, CancellationToken cancellationToken)
    {
        foreach (var organization in _organizations.Where(x => x.ChangeId >= startAtChangeId))
        {
            await handler(organization);
        }
    }
}
