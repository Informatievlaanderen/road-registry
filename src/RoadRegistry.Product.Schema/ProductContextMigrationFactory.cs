namespace RoadRegistry.Product.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;

    public class ProductContextMigrationFactory : RunnerDbContextMigrationFactory<ProductContext>
    {
        public ProductContextMigrationFactory() :
            base(WellknownConnectionNames.ProductProjectionsAdmin, HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = WellknownSchemas.ProductSchema,
                Table = MigrationTables.Product
            };

        protected override ProductContext CreateContext(DbContextOptions<ProductContext> migrationContextOptions)
            => new ProductContext(migrationContextOptions);
    }
}
