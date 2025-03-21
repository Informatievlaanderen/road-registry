namespace RoadRegistry.Sync.MunicipalityRegistry;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;

public class MunicipalityEventConsumerContext : SqlServerConsumerDbContext<MunicipalityEventConsumerContext>, IOffsetOverrideDbSet
{
    private const string ConsumerSchema = WellKnownSchemas.MunicipalityEventConsumerSchema;

    public override string ProcessedMessagesSchema => ConsumerSchema;

    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<OffsetOverride> OffsetOverrides => Set<OffsetOverride>();

    public MunicipalityEventConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public MunicipalityEventConsumerContext(DbContextOptions<MunicipalityEventConsumerContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OffsetOverrideConfiguration(ConsumerSchema));
        modelBuilder.ApplyConfiguration(new MunicipalityConfiguration());
    }

    public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
    {
        options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.MunicipalityEventConsumer),
                sqlOptions => sqlOptions
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite()
                    .MigrationsHistoryTable(MigrationTables.MunicipalityEventConsumer, WellKnownSchemas.MunicipalityEventConsumerSchema));
    }

    public async Task<Municipality?> FindCurrentMunicipalityByNisCode(string nisCode, CancellationToken cancellationToken)
    {
        return await Municipalities.IncludeLocalSingleOrDefaultAsync(x => x.NisCode == nisCode && x.Status == MunicipalityStatus.Current, cancellationToken);
    }

    public async Task<bool> CurrentMunicipalityExistsByNisCode(string nisCode, CancellationToken cancellationToken)
    {
        return await Municipalities.AnyAsync(x => x.NisCode == nisCode && x.Status == MunicipalityStatus.Current, cancellationToken);
    }
}

public class MunicipalityEventConsumerContextMigrationFactory : DbContextMigratorFactory<MunicipalityEventConsumerContext>
{
    public MunicipalityEventConsumerContextMigrationFactory()
        : base(WellKnownConnectionNames.MunicipalityEventConsumerAdmin, new MigrationHistoryConfiguration
        {
            Schema = WellKnownSchemas.MunicipalityEventConsumerSchema,
            Table = MigrationTables.MunicipalityEventConsumer
        })
    {
    }

    protected override MunicipalityEventConsumerContext CreateContext(DbContextOptions<MunicipalityEventConsumerContext> migrationContextOptions)
    {
        return new MunicipalityEventConsumerContext(migrationContextOptions);
    }

    protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        base.ConfigureSqlServerOptions(sqlServerOptions);

        sqlServerOptions.UseNetTopologySuite();
    }
}
