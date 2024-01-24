namespace RoadRegistry.Sync.StreetNameRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

public class StreetNameProjectionContext : RunnerDbContext<StreetNameProjectionContext>
{
    public override string ProjectionStateSchema => WellKnownSchemas.StreetNameSchema;

    public StreetNameProjectionContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameProjectionContext(DbContextOptions<StreetNameProjectionContext> options)
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
                sp.GetRequiredService<TraceDbConnection<StreetNameProjectionContext>>(),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetName, WellKnownSchemas.StreetNameSchema));
    }
}

public class StreetNameProjectionContextMigrationFactory : DbContextMigratorFactory<StreetNameProjectionContext>
{
    public StreetNameProjectionContextMigrationFactory()
        : base(WellKnownConnectionNames.StreetNameProjectionsAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.StreetNameSchema,
            Table = MigrationTables.StreetName
        })
    {
    }
    
    protected override StreetNameProjectionContext CreateContext(DbContextOptions<StreetNameProjectionContext> migrationContextOptions)
    {
        return new StreetNameProjectionContext(migrationContextOptions);
    }
}
