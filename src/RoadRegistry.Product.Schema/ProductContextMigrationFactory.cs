namespace RoadRegistry.Product.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class ProductContextMigrationFactory : RunnerDbContextMigrationFactory<ProductContext>
{
    public ProductContextMigrationFactory() :
        base(WellKnownConnectionNames.ProductProjectionsAdmin, HistoryConfiguration)
    {
    }

    private static MigrationHistoryConfiguration HistoryConfiguration =>
        new()
        {
            Schema = WellKnownSchemas.ProductSchema,
            Table = MigrationTables.Product
        };
    
    protected override ProductContext CreateContext(DbContextOptions<ProductContext> migrationContextOptions)
    {
        return new ProductContext(migrationContextOptions);
    }
}
