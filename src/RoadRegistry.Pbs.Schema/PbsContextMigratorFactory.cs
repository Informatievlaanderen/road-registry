namespace RoadRegistry.Pbs.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class PbsContextMigratorFactory : DbContextMigratorFactory<PbsContext>
{
    public static readonly MigrationHistoryConfiguration Configuration = new()
    {
        Schema = WellKnownSchemas.PbsSchema,
        Table = MigrationTables.Pbs
    };

    public PbsContextMigratorFactory()
        : base(WellKnownConnectionNames.PbsProjectionsAdmin, Configuration)
    {
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        PbsContext.ConfigureSqlServerOptions(sqlServerOptions);
    }

    protected override PbsContext CreateContext(DbContextOptions<PbsContext> migrationContextOptions)
    {
        return new PbsContext(migrationContextOptions);
    }
}
