namespace RoadRegistry.Product.Schema;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class FakeProductContextFactory : IDesignTimeDbContextFactory<FakeProductContext>
{
    public FakeProductContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ProductContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        return new FakeProductContext(builder.Options);
    }
}
