namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System;

public class StreetNameSnapshotProjectionContext : RunnerDbContext<StreetNameSnapshotProjectionContext>
{
    public override string ProjectionStateSchema => WellKnownSchemas.StreetNameSchema;

    public StreetNameSnapshotProjectionContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameSnapshotProjectionContext(DbContextOptions<StreetNameSnapshotProjectionContext> options)
        : base(options)
    {
    }
    
    public DbSet<StreetNameRecord> StreetNames { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
    
    internal static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<TraceDbConnection<StreetNameSnapshotProjectionContext>>(),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetName, WellKnownSchemas.StreetNameSchema));
    }
}

public class StreetNameSnapshotProjectionContextMigrationFactory : DbContextMigratorFactory<StreetNameSnapshotProjectionContext>
{
    public StreetNameSnapshotProjectionContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameProjectionsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameSchema,
            Table = MigrationTables.StreetName
        })
    {
    }
    
    protected override StreetNameSnapshotProjectionContext CreateContext(DbContextOptions<StreetNameSnapshotProjectionContext> migrationContextOptions)
    {
        return new StreetNameSnapshotProjectionContext(migrationContextOptions);
    }
}
