namespace RoadRegistry.Extracts.Schema;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class ExtractsDbContext : RunnerDbContext<ExtractsDbContext>
{
    public ExtractsDbContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public ExtractsDbContext(DbContextOptions<ExtractsDbContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.ExtractsSchema;

    public DbSet<ExtractRequest> ExtractRequests { get; set; }
    public DbSet<ExtractDownload> ExtractDownloads { get; set; }
    public DbSet<ExtractUpload> ExtractUploads { get; set; }
    public DbSet<ExtractUploadStatusHistory> ExtractUploadStatusHistory { get; set; }
    public DbSet<Inwinningszone> Inwinningszones { get; set; }
    public DbSet<InwinningRoadSegment> InwinningRoadSegments { get; set; }
    public DbSet<DataValidationQueueItem> DataValidationQueue { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }
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

    public async Task<bool> HasInwinningRoadSegments(IEnumerable<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        const int batchSize = 2000; // SQL Server handles ~2100 params well
        var segmentIdsList = roadSegmentIds.Distinct().ToList();

        for (var i = 0; i < segmentIdsList.Count; i += batchSize)
        {
            var batch = segmentIdsList.Skip(i).Take(batchSize).Select(x => x.ToInt32());
            var hasInwinning = await InwinningRoadSegments
                .AnyAsync(x => batch.Contains(x.RoadSegmentId), cancellationToken: cancellationToken);
            if (hasInwinning)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<IReadOnlyCollection<RoadSegmentId>> GetInwinningRoadSegmentIds(IEnumerable<RoadSegmentId> roadSegmentIds, CancellationToken cancellationToken)
    {
        const int batchSize = 2000; // SQL Server handles ~2100 params well
        var segmentIdsList = roadSegmentIds.Distinct().ToList();

        var inwinningRoadSegmentIds = new List<RoadSegmentId>();

        for (var i = 0; i < segmentIdsList.Count; i += batchSize)
        {
            var batch = segmentIdsList.Skip(i).Take(batchSize).Select(x => x.ToInt32());
            var inwinningIds = await InwinningRoadSegments
                .Where(x => batch.Contains(x.RoadSegmentId))
                .Select(x => x.RoadSegmentId)
                .ToListAsync(cancellationToken);
            inwinningRoadSegmentIds.AddRange(inwinningIds.Select(x => new RoadSegmentId(x)));
        }

        return inwinningRoadSegmentIds;
    }

    public async Task<IReadOnlyCollection<RoadSegmentId>> CheckWhichOverlapWithInwinningszone(IEnumerable<(MultiLineString Geometry, RoadSegmentId TemporaryId)> roadSegments, CancellationToken cancellationToken)
    {
        var inwinningszonesGeometries = await Inwinningszones
            .AsNoTracking()
            .Select(x => x.Contour)
            .ToListAsync(cancellationToken);

        var preparedZones = inwinningszonesGeometries
            .Select(NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare)
            .ToList();

        var overlappingIds = roadSegments
            .AsParallel()
            .Where(segment => preparedZones.Any(zone => zone.Intersects(segment.Geometry)))
            .Select(x => x.TemporaryId)
            .ToList();

        return overlappingIds;
    }

    public async Task UploadAcceptedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, async record =>
        {
            record.Status = ExtractUploadStatus.Accepted;

            var extractDownload = await ExtractDownloads.SingleAsync(x => x.DownloadId == record.DownloadId, cancellationToken);
            extractDownload.Close();
        }, cancellationToken);
    }

    public async Task AutomaticValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, record =>
        {
            record.Status = ExtractUploadStatus.AutomaticValidationFailed;
            return Task.CompletedTask;
        }, cancellationToken);
    }

    public async Task AutomaticValidationSucceededAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, record =>
        {
            record.Status = ExtractUploadStatus.AutomaticValidationSucceeded;
            return Task.CompletedTask;
        }, cancellationToken);
    }

    public async Task ManualValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, record =>
        {
            record.Status = ExtractUploadStatus.ManualValidationFailed;
            return Task.CompletedTask;
        }, cancellationToken);
    }

    private async Task UpdateExtractUpload(UploadId uploadId, Func<ExtractUpload, Task> change, CancellationToken cancellationToken)
    {
        var record = await ExtractUploads.SingleOrDefaultAsync(x => x.UploadId == uploadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            throw new UploadExtractNotFoundException($"Could find extractupload with uploadId {uploadId}");
        }

        var currentStatus = record.Status;
        await change(record);
        if (currentStatus != record.Status)
        {
            ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
            {
                UploadId = uploadId,
                Status = record.Status,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        await SaveChangesAsync(cancellationToken);
    }
}

public static class ExtractsDbContextExtensions
{
    public static IServiceCollection AddExtractsDbContext(this IServiceCollection services, QueryTrackingBehavior queryTrackingBehavior)
    {
        return services
            .AddDbContext<ExtractsDbContext>((sp, options) => options
                .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .UseQueryTrackingBehavior(queryTrackingBehavior)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(WellKnownConnectionNames.Extracts, WellKnownConnectionNames.RoadRegistry),
                    ExtractsDbContext.ConfigureSqlServerOptions));
    }

    public static IServiceCollection AddExtractsDbContextFactory(this IServiceCollection services, QueryTrackingBehavior queryTrackingBehavior, string connectionStringName)
    {
        return services
            .AddDbContextFactory<ExtractsDbContext>((sp, options) => options
                .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .UseQueryTrackingBehavior(queryTrackingBehavior)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString(connectionStringName, WellKnownConnectionNames.RoadRegistry),
                    ExtractsDbContext.ConfigureSqlServerOptions));
    }
}
