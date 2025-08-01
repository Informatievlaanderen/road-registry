namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class ExtractsDbContextMigratorFactory : DbContextMigratorFactory<ExtractsDbContext>
{
    public ExtractsDbContextMigratorFactory()
        : base(WellKnownConnectionNames.ExtractsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.ExtractsSchema,
            Table = MigrationTables.Default
        })
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
    }

    protected override ExtractsDbContext CreateContext(DbContextOptions<ExtractsDbContext> migrationContextOptions)
    {
        return new ExtractsDbContext(migrationContextOptions);
    }
}
