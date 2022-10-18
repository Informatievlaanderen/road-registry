namespace RoadRegistry.Product.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;

public class ProductContextMigrationFactory : RunnerDbContextMigrationFactory<ProductContext>
{
    public ProductContextMigrationFactory() :
        base(WellknownConnectionNames.ProductProjectionsAdmin, HistoryConfiguration)
    {
    }

    protected override ProductContext CreateContext(DbContextOptions<ProductContext> migrationContextOptions)
    {
        return new ProductContext(migrationContextOptions);
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellknownSchemas.ProductSchema,
            Table = MigrationTables.Product
        };
}
