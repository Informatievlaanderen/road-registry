namespace RoadRegistry.Extracts.Schema;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class FakeExtractsDbContextFactory// : IDesignTimeDbContextFactory<ExtractsDbContext>
{
    public ExtractsDbContext CreateDbContext(params string[] args)
    {
        var builder = new DbContextOptionsBuilder<ExtractsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        return new ExtractsDbContext(builder.Options);
    }
}
