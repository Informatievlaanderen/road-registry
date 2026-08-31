namespace RoadRegistry.Tests.RunOnce;

using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Api.Extracten;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Sync.StreetNameRegistry;

/// <summary>
/// Runs <see cref="RoadSegmentsZipArchiveWriter"/> against a real environment, using a local .shp file as the extract contour.
/// Fill in <see cref="ShpFilePath"/> / <see cref="OutputZipFilePath"/>, pick the <see cref="DbEnvironment"/>, swap the [Fact(Skip)] attribute and run.
/// </summary>
public class WriteRoadSegmentsZipArchiveV2
{
    private const string ShpFilePath = @"C:\Users\RikDePeuter\Downloads\Contour.shp";
    private const string OutputZipFilePath = @"Wegsegment.zip";
    private const DbEnvironment Environment = DbEnvironment.PRD;
    private const bool IsInformative = false;

    public WriteRoadSegmentsZipArchiveV2(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    private IConfiguration Configuration { get; }
    private ITestOutputHelper TestOutputHelper { get; }

    [Fact]
    //[Fact(Skip = "For debugging purposes only")]
    public async Task Run()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var contour = ReadContour(ShpFilePath);
        TestOutputHelper.WriteLine($"Contour: SRID {contour.SRID}, {contour.NumGeometries} polygon(s), area {contour.Area:F2}");

        await using var editorContext = CreateEditorContext();
        await using var extractsDbContext = CreateExtractsDbContext();

        var zipArchiveDataProvider = new ZipArchiveDataProvider(editorContext, extractsDbContext);

        // purely informational, the data provider caches these per contour so the writer does not query twice
        var segments = await zipArchiveDataProvider.GetRoadSegments(contour, CancellationToken.None);
        var hasInwinningRoadSegment = await zipArchiveDataProvider.HasInwinningRoadSegment(contour, CancellationToken.None);
        TestOutputHelper.WriteLine($"Road segments inside contour: {segments.Count}");
        TestOutputHelper.WriteLine($"Has inwinning road segment (writer writes 0 segments when true): {hasInwinningRoadSegment}");

        var writer = new RoadSegmentsZipArchiveWriter(
            CreateStreetNameCache(),
            new RecyclableMemoryStreamManager(),
            FileEncoding.WindowsAnsi);

        var request = new RoadNetworkExtractAssemblyRequest(
            new DownloadId(Guid.NewGuid()),
            new ExtractDescription("debug"),
            contour,
            IsInformative,
            WellKnownZipArchiveWriterVersions.DomainV1_2);

        await using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Update, true, FileEncoding.WindowsAnsi))
        {
            await writer.WriteAsync(archive, request, zipArchiveDataProvider, CancellationToken.None);

            foreach (var entry in archive.Entries)
            {
                TestOutputHelper.WriteLine($"{entry.FullName}: {entry.Length} bytes");
            }
        }

        ms.Position = 0;
        await File.WriteAllBytesAsync(OutputZipFilePath, ms.ToArray());
        TestOutputHelper.WriteLine($"Written to {OutputZipFilePath}");
    }

    private MultiPolygon ReadContour(string shpFilePath)
    {
        using var shpStream = File.OpenRead(shpFilePath);

        return new ExtractShapefileContourReader()
            .Read(shpStream, WellKnownGeometryFactories.Lambert72WithoutMAndZ)
            .ToMultiPolygon();
    }

    private string GetConnectionString()
    {
        return Configuration.GetConnectionString($"RoadRegistry-{Environment}")
               ?? Configuration.GetConnectionString($"EditorProjections-{Environment}")
               ?? Configuration.GetRequiredConnectionString(WellKnownConnectionNames.RoadRegistry);
    }

    private EditorContext CreateEditorContext()
    {
        return new EditorContext(new DbContextOptionsBuilder<EditorContext>()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(GetConnectionString(), options => options.UseNetTopologySuite())
            .Options);
    }

    private ExtractsDbContext CreateExtractsDbContext()
    {
        return new ExtractsDbContext(new DbContextOptionsBuilder<ExtractsDbContext>()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(GetConnectionString(), options => options.UseNetTopologySuite())
            .Options);
    }

    private IStreetNameCache CreateStreetNameCache()
    {
        var connectionString = GetConnectionString();

        return new StreetNameCache(
            new SimpleDbContextFactory<StreetNameSnapshotProjectionContext>(
                new DbContextOptionsBuilder<StreetNameSnapshotProjectionContext>()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(connectionString)
                    .Options),
            new SimpleDbContextFactory<StreetNameEventProjectionContext>(
                new DbContextOptionsBuilder<StreetNameEventProjectionContext>()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseSqlServer(connectionString)
                    .Options));
    }

    private sealed class SimpleDbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly DbContextOptions<TContext> _options;

        public SimpleDbContextFactory(DbContextOptions<TContext> options)
        {
            _options = options;
        }

        public TContext CreateDbContext()
        {
            return (TContext)Activator.CreateInstance(typeof(TContext), _options);
        }
    }

    private enum DbEnvironment
    {
        DEV,
        TST,
        STG,
        PRD
    }
}
