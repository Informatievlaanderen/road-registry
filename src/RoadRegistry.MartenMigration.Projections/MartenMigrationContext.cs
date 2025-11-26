namespace RoadRegistry.MartenMigration.Projections;

using System;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class MartenMigrationContext : RunnerDbContext<MartenMigrationContext>
{
    public MartenMigrationContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public MartenMigrationContext(DbContextOptions<MartenMigrationContext> options)
        : base(options)
    {
        if (!Database.IsInMemory())
        {
            Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        }
    }

    public override string ProjectionStateSchema => WellKnownSchemas.MartenMigrationSchema;

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.MartenMigrationAdmin),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.Default, WellKnownSchemas.MartenMigrationSchema));
    }
}

public class MartenMigrationContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<MartenMigrationContext>
{
    public MartenMigrationContextMigrationFactory()
        : base(WellKnownConnectionNames.MartenMigrationAdmin, new()
        {
            Schema = WellKnownSchemas.MartenMigrationSchema,
            Table = MigrationTables.Default
        })
    {
    }

    protected override MartenMigrationContext CreateContext(DbContextOptions<MartenMigrationContext> migrationContextOptions)
    {
        return new MartenMigrationContext(migrationContextOptions);
    }
}
