namespace RoadRegistry.BackOffice.Projections
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Schema;

    public static class MigrationsHelper
    {
        public static void Run(string connectionString, ILoggerFactory loggerFactory = null)
        {
            var migratorOptions = new DbContextOptionsBuilder<ShapeContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Shape, Schema.ProjectionMetaData);
                    });

            if (loggerFactory != null)
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            using (var migrator = new ShapeContext(migratorOptions.Options))
                migrator.Database.Migrate();
        }
    }
}
