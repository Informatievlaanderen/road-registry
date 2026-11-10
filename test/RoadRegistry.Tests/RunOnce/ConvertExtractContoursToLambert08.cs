namespace RoadRegistry.Tests.RunOnce;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.ValueObjects;

// Extract contours are written in Lambert 2008 from now on, but the rows that predate that are in Lambert 72. Lambert
// 72 to Lambert 2008 is a datum transformation, not a shift, so SQL Server cannot do it and an EF migration cannot
// carry it: it is run from here instead, once per environment, after the deploy.
//
// Safe to run more than once: a contour that is already Lambert 2008 is left alone. Run it while nothing is writing
// extract downloads, so a request cannot slip in between reading a row and writing it back.
public class ConvertExtractContoursToLambert08
{
    public ConvertExtractContoursToLambert08(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    private IConfiguration Configuration { get; }
    private ITestOutputHelper TestOutputHelper { get; }

    //[Fact]
    [Fact(Skip = "Run once per environment, after deploying GAWR-7199")]
    public async Task Run()
    {
        const DbEnvironment env = DbEnvironment.TST;
        // const DbEnvironment env = DbEnvironment.STG;
        // const DbEnvironment env = DbEnvironment.PRD;
        const bool dryRun = true;

        var sp = GetServiceProvider(env);
        await using var scope = sp.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExtractsDbContext>();

        var extractDownloads = await dbContext.ExtractDownloads.ToListAsync();

        var converted = 0;
        foreach (var extractDownload in extractDownloads)
        {
            if (extractDownload.Contour.SRID != WellknownSrids.Lambert72)
            {
                continue;
            }

            extractDownload.Contour = extractDownload.Contour.TransformFromLambert72To08();
            converted++;
        }

        TestOutputHelper.WriteLine($"{env}: {converted} of {extractDownloads.Count} extract download contours are in Lambert 72.");

        if (dryRun)
        {
            TestOutputHelper.WriteLine("Dry run: nothing was written. Set dryRun to false to convert them.");
            return;
        }

        await dbContext.SaveChangesAsync();
        TestOutputHelper.WriteLine($"{converted} contours converted to Lambert 2008.");
    }

    private IServiceProvider GetServiceProvider(DbEnvironment environment)
    {
        var services = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddDbContext<ExtractsDbContext>((sp, options) => options
                .UseLoggerFactory(new NullLoggerFactory())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .UseSqlServer(
                    sp.GetRequiredService<IConfiguration>().GetRequiredConnectionString($"{WellKnownConnectionNames.RoadRegistry}-{environment}"),
                    sqlOptions => sqlOptions
                        .UseNetTopologySuite())
            );

        return services.BuildServiceProvider();
    }

    private enum DbEnvironment
    {
        DEV,
        TST,
        STG,
        PRD
    }
}
