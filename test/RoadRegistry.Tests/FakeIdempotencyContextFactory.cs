namespace RoadRegistry.Tests
{
    using System;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public sealed class FakeIdempotencyContextFactory : IDesignTimeDbContextFactory<IdempotencyContext>
    {
        public IdempotencyContext CreateDbContext(params string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdempotencyContext>();
            var tableInfo = new IdempotencyTableInfo("dbo");

            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new IdempotencyContext(optionsBuilder.Options, tableInfo);
        }
    }
}
