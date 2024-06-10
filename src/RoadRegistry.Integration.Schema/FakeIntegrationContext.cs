namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;

public sealed class FakeIntegrationContext : IntegrationContext
{
    public FakeIntegrationContext()
    { }

    public FakeIntegrationContext(DbContextOptions<IntegrationContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
}
