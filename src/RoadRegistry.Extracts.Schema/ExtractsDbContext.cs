namespace RoadRegistry.Extracts.Schema;

using System.Reflection;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ExtractsDbContext : DbContext
{
    public ExtractsDbContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public ExtractsDbContext(DbContextOptions<ExtractsDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExtractRequest> ExtractRequests { get; set; }
    public DbSet<ExtractDownload> ExtractDownloads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly);
    }

    internal static void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
    }
}

public static class ExtractsDbContextExtensions
{
    public static IServiceCollection AddExtractsDbContext(this IServiceCollection services, QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.NoTracking)
    {
        return services.AddDbContext<ExtractsDbContext>((sp, options) => options
            .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
            .UseQueryTrackingBehavior(queryTrackingBehavior)
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.Extracts),
                ExtractsDbContext.ConfigureSqlServerOptions));
    }
}
