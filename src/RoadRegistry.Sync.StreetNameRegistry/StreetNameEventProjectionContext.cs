namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;

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
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.StreetNameProjections),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameEvent, WellKnownSchemas.StreetNameEventSchema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do not call base method to avoid ApplyConfigurationsFromAssembly
        //base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
        modelBuilder.ApplyConfiguration(new RenamedStreetNameRecordEntityTypeConfiguration());
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
