namespace RoadRegistry.Tests.RunOnce;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.Extensions;
using RoadRegistry.Sync.MunicipalityRegistry;
using RoadRegistry.Sync.MunicipalityRegistry.Models;

public class InwinningSplitMunicipalityIntoGrids
{
    public InwinningSplitMunicipalityIntoGrids(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    private IConfiguration Configuration { get; }
    private ITestOutputHelper TestOutputHelper { get; }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task Run()
    {
        const DbEnvironment env = DbEnvironment.DEV;
        // const DbEnvironment env = DbEnvironment.STG;
        const string nisCode = "44021"; //Gent 44021  Kruisem 45068
        var gridSizes = new[] { 500 }; //, 500, 1000 };
        var orgCode = "0425258688";
        var configOrgStartIndex = 10;
        var maxMunisPerGridSize = 10;

        var sp = GetServiceProvider(env);

        var dbContext = sp.GetRequiredService<MunicipalityEventConsumerContext>();

        var municipality = await dbContext.Municipalities.SingleAsync(m => m.NisCode == nisCode);
        var municipalityGeometry = municipality.Geometry!.EnvelopeInternal;

        var y = Math.Round((municipalityGeometry.MaxY + municipalityGeometry.MinY) / 2 / 1000.0) * 1000;
        var minX = Math.Round(municipalityGeometry.MinX / 1000.0) * 1000;

        var gridNisCodes = new List<string>();

        foreach (var gridSize in gridSizes)
        {
            var createdCount = 0;
            var x = minX;

            while (x < municipalityGeometry.MaxX)
            {
                if (createdCount == maxMunisPerGridSize)
                {
                    break;
                }

                var gridGeometry = municipality.Geometry.Factory.CreatePolygon([
                    new Coordinate(x, y),
                    new Coordinate(x, y + gridSize),
                    new Coordinate(x + gridSize, y + gridSize),
                    new Coordinate(x + gridSize, y),
                    new Coordinate(x, y),
                ]).ToMultiPolygon();

                var gridNiscode = $"{municipality.NisCode}_{x:000000}-{y:000000}_{gridSize}x{gridSize}";
                var existingGrid = await dbContext.Municipalities.SingleOrDefaultAsync(m => m.NisCode == gridNiscode);
                if (existingGrid is null)
                {
                    dbContext.Municipalities.Add(new Municipality
                    {
                        MunicipalityId = Guid.NewGuid().ToString(),
                        NisCode = gridNiscode,
                        Geometry = gridGeometry,
                        Status = MunicipalityStatus.Current
                    });

                    gridNisCodes.Add(gridNiscode);
                    createdCount++;
                }

                x += gridSize;
            }

            y -= 1000;
        }

        await dbContext.SaveChangesAsync();

        TestOutputHelper.WriteLine($"Added grids: {gridNisCodes.Count}");
        TestOutputHelper.WriteLine("");
        TestOutputHelper.WriteLine("API appdef config:");
        foreach (var gridNisCode in gridNisCodes)
        {
            TestOutputHelper.WriteLine($@"        {{
          ""name"": ""InwinningOrganizationNisCodes__{orgCode}__{configOrgStartIndex}"",
          ""value"": ""{gridNisCode}""
        }},");

            configOrgStartIndex++;
        }
    }

    private IServiceProvider GetServiceProvider(DbEnvironment environment)
    {
        var services = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddDbContext<MunicipalityEventConsumerContext>((sp, options) => options
                .UseLoggerFactory(new NullLoggerFactory())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
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
        STG
    }
}
