namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System;

public class StreetNameEventProjectionContext : RunnerDbContext<StreetNameEventProjectionContext>
{
    public override string ProjectionStateSchema => WellKnownSchemas.StreetNameEventSchema;

    public StreetNameEventProjectionContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameEventProjectionContext(DbContextOptions<StreetNameEventProjectionContext> options)
        : base(options)
    {
    }

    public DbSet<RenamedStreetNameRecord> RenamedStreetNames { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    internal static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<TraceDbConnection<StreetNameEventProjectionContext>>(),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameEvent, WellKnownSchemas.StreetNameEventSchema));
    }
}

public class StreetNameEventProjectionContextMigrationFactory : DbContextMigratorFactory<StreetNameEventProjectionContext>
{
    public StreetNameEventProjectionContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameProjectionsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameEventSchema,
            Table = MigrationTables.StreetNameEvent
        })
    {
    }
    
    protected override StreetNameEventProjectionContext CreateContext(DbContextOptions<StreetNameEventProjectionContext> migrationContextOptions)
    {
        return new StreetNameEventProjectionContext(migrationContextOptions);
    }
}
