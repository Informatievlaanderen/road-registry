namespace RoadRegistry.Product.Schema;

using Microsoft.EntityFrameworkCore;

public sealed class FakeProductContext : ProductContext
{
    public FakeProductContext()
    {
    }

    public FakeProductContext(DbContextOptions<ProductContext> options) : base(options)
    {
    }
}