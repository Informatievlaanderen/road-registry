namespace RoadRegistry.Projections.Oslo
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public static class MigrationsHelper
    {
        public static void Run(string connectionString, ILoggerFactory loggerFactory = null)
        {
            var migratorOptions = new DbContextOptionsBuilder<OsloContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Oslo, Schema.Oslo);
                    });

            if (loggerFactory != null)
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            using (var migrator = new OsloContext(migratorOptions.Options))
                migrator.Database.Migrate();
        }
    }
}
