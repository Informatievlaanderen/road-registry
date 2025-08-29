namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class ExtractsDbContextMigratorFactory : DbContextMigratorFactory<ExtractsDbContext>
{
    public static readonly MigrationHistoryConfiguration Configuration = new()
    {
        Schema = WellKnownSchemas.ExtractsSchema,
        Table = MigrationTables.Default
    };

    public ExtractsDbContextMigratorFactory()
        : base(WellKnownConnectionNames.ExtractsAdmin, Configuration)
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        ExtractsDbContext.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override ExtractsDbContext CreateContext(DbContextOptions<ExtractsDbContext> migrationContextOptions)
    {
        return new ExtractsDbContext(migrationContextOptions);
    }
}
