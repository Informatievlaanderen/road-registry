namespace RoadRegistry.Integration.Schema;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class FakeIntegrationContextFactory : IDesignTimeDbContextFactory<FakeIntegrationContext>
{
    public FakeIntegrationContext CreateDbContext(params string[] args)
    {
        var builder = new DbContextOptionsBuilder<IntegrationContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new FakeIntegrationContext(builder.Options);
    }
}
