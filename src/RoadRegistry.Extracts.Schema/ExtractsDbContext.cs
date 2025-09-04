namespace RoadRegistry.Extracts.Schema;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

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

    public async Task<List<Guid>> GetOverlappingExtractDownloadIds(Geometry geometry, CancellationToken cancellationToken)
    {
        geometry = (Geometry)GeometryTranslator.Translate(GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)geometry, -0.001));

        var extractDownloadsQuery = ExtractDownloads
            .AsNoTracking()
            .Where(x => !x.IsInformative && !x.Closed)
            ;

        var overlaps = await (
            from extractDownload in extractDownloadsQuery
            let intersection = extractDownload.Contour.Intersection(geometry)
            where intersection != null
            select new { overlap = extractDownload, intersection }
        ).ToListAsync(cancellationToken);

        var downloadIds = overlaps
            .Where(x => (x.intersection is Polygon polygon && polygon.Area > 0)
                        || (x.intersection is MultiPolygon multiPolygon && multiPolygon.Area > 0))
            .Select(x => x.overlap.DownloadId)
            .Distinct()
            .ToList();

        return downloadIds;
    }
}

public static class ExtractsDbContextExtensions
{
    public static IServiceCollection AddExtractsDbContext(this IServiceCollection services, QueryTrackingBehavior queryTrackingBehavior)
    {
        return services.AddDbContext<ExtractsDbContext>((sp, options) => options
            .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
            .UseQueryTrackingBehavior(queryTrackingBehavior)
            .UseSqlServer(
                sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.Extracts),
                ExtractsDbContext.ConfigureSqlServerOptions));
    }
}
