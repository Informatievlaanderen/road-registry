namespace RoadRegistry.Integration.Schema;

using Microsoft.EntityFrameworkCore;

public sealed class FakeIntegrationContext : IntegrationContext
{
    public FakeIntegrationContext()
    {
    }

    public FakeIntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    {
    }
}
