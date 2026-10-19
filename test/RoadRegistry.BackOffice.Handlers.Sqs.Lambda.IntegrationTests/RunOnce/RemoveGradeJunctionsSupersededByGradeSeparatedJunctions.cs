namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Dapper;
using Marten;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Npgsql;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using Xunit.Abstractions;

/// <summary>
/// Before the fix, VerifyAndUpdateJunctions judged a crossing on the traffic attributes of both road segments without
/// checking that both had finished their inwinning. A segment that is still V1 has no traffic attributes, so it read as
/// carrying no traffic at all, the crossing looked gelijkgronds, and a gelijkgrondse kruising was recorded next to the
/// ongelijkgrondse kruising that already described that very crossing. A segment pair never carries both kinds of
/// kruising at once, so those gelijkgrondse kruisingen are wrong and this removes them.
///
/// The candidates are found on the topology projection as the gelijkgrondse kruisingen whose (unordered) road segment
/// pair also carries an ongelijkgrondse kruising. Every candidate is judged again on its aggregates before anything is
/// removed, by the domain itself: ScopedRoadNetwork.RemoveGradeJunctionsSupersededByGradeSeparatedJunctions appends a
/// GradeJunctionWasRemoved per kruising, followed by one RoadNetworkWasChanged per scope.
///
/// Leave <c>dryRun</c> true to only list what it would remove - nothing is written then, not even a read-model update.
/// This WRITES when <c>dryRun</c> is false: the events land in the configured event store like a real change. Run it on
/// STG before PRD, and after the fix is deployed.
/// </summary>
public class RemoveGradeJunctionsSupersededByGradeSeparatedJunctions
{
    private const int BatchSize = 100;

    private readonly ITestOutputHelper _outputHelper;

    public RemoveGradeJunctionsSupersededByGradeSeparatedJunctions(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    //[Fact]
    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // Only lists what it would remove; set to false to write.
        var dryRun = true;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();
        var repository = new RoadNetworkRepository(store);
        var provenance = new RoadRegistryProvenanceData(
            Modification.Update,
            reason: "Remove grade junctions superseded by a grade separated junction").ToProvenance();

        var candidateIds = await FindCandidates(store);
        _outputHelper.WriteLine($"Candidates: {candidateIds.Count}");
        _outputHelper.WriteLine(string.Empty);

        var removed = new HashSet<GradeJunctionId>();

        foreach (var batch in candidateIds.Chunk(BatchSize))
        {
            // A scope is loaded around a candidate's road segments, so it can already have taken care of candidates
            // from a later batch that sit on those same segments.
            var pending = batch.Where(x => !removed.Contains(x)).ToArray();
            if (pending.Length == 0)
            {
                continue;
            }

            await using var session = store.LightweightSession();

            var ids = await repository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [], [], pending));
            var roadNetwork = await repository.Load(session, ids, new ScopedRoadNetworkId(Guid.NewGuid()));

            // What the domain judges to be superseded, reported before anything is removed.
            foreach (var gradeJunction in roadNetwork.GetGradeJunctionsSupersededByGradeSeparatedJunctions())
            {
                var gradeSeparatedJunction = roadNetwork.FindGradeSeparatedJunctionFor(gradeJunction);
                _outputHelper.WriteLine(
                    $"{(dryRun ? "Would remove" : "Removing")} grade junction {gradeJunction.GradeJunctionId}: " +
                    $"road segments {gradeJunction.RoadSegmentId1} and {gradeJunction.RoadSegmentId2}, " +
                    $"superseded by grade separated junction {gradeSeparatedJunction?.GradeSeparatedJunctionId} ({gradeSeparatedJunction?.Type?.ToString() ?? "V1"})");
            }

            var result = roadNetwork.RemoveGradeJunctionsSupersededByGradeSeparatedJunctions(provenance);
            if (result.Problems.HasError())
            {
                throw new InvalidOperationException($"Removing grade junctions {string.Join(", ", pending)} failed: {string.Join(", ", result.Problems.Select(x => x.Describe()))}");
            }

            foreach (var gradeJunctionId in result.Summary.GradeJunctions.Removed)
            {
                removed.Add(gradeJunctionId);
            }

            if (dryRun || result.Summary.GradeJunctions.Removed.Count == 0)
            {
                continue;
            }

            repository.Save(session, roadNetwork, nameof(RemoveGradeJunctionsSupersededByGradeSeparatedJunctions));
            await session.SaveChangesAsync();
        }

        // A candidate the aggregates disagree about: the projection still pairs it with an ongelijkgrondse kruising
        // while the aggregate no longer does, or the kruising is already removed. Worth looking at, not worth failing.
        var untouched = candidateIds.Where(x => !removed.Contains(x)).ToArray();

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"{(dryRun ? "Would remove" : "Removed")}: {removed.Count}");
        _outputHelper.WriteLine($"Left alone: {untouched.Length}{(untouched.Length > 0 ? $" ({string.Join(", ", untouched)})" : string.Empty)}");
    }

    // The topology projection is inline, so it is as current as the aggregates. It only preselects: every candidate is
    // judged again on its aggregate before it is removed.
    private static async Task<IReadOnlyList<GradeJunctionId>> FindCandidates(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var ids = await session.Connection!.QueryAsync<int>($@"
SELECT DISTINCT gradejunction.id
FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} gradejunction
JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} gradeseparatedjunction
  ON LEAST(gradeseparatedjunction.lower_road_segment_id, gradeseparatedjunction.upper_road_segment_id)
   = LEAST(gradejunction.road_segment_id_1, gradejunction.road_segment_id_2)
 AND GREATEST(gradeseparatedjunction.lower_road_segment_id, gradeseparatedjunction.upper_road_segment_id)
   = GREATEST(gradejunction.road_segment_id_1, gradejunction.road_segment_id_2)
ORDER BY gradejunction.id");

        return ids.Select(x => new GradeJunctionId(x)).ToList();
    }

    private void PrintTargets(IConfiguration configuration)
    {
        _outputHelper.WriteLine("Targets:");
        _outputHelper.WriteLine($"  Marten (aggregates): {DescribePostgres(configuration.GetConnectionString(WellKnownConnectionNames.Marten))}");
        _outputHelper.WriteLine($"  Events (id sequences): {DescribeSqlServer(configuration.GetConnectionString(WellKnownConnectionNames.Events) ?? configuration.GetConnectionString(WellKnownConnectionNames.RoadRegistryEvents))}");
        _outputHelper.WriteLine(string.Empty);
    }

    private static string DescribePostgres(string? connectionString)
    {
        if (connectionString is null)
        {
            return "<not set>";
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{builder.Host}:{builder.Port}/{builder.Database}";
    }

    private static string DescribeSqlServer(string? connectionString)
    {
        if (connectionString is null)
        {
            return "<not set>";
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        return $"{builder.DataSource}/{builder.InitialCatalog}";
    }

    // See ForDebugging in WhenChangingRoadSegmentStatusV2: the machine-specific 'RoadRegistryEvents' has to win over the
    // 'Events' that appsettings.json pins at the local docker-compose SQL Server.
    private static IConfiguration BuildConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

        var roadRegistryEvents = configuration.GetConnectionString(WellKnownConnectionNames.RoadRegistryEvents);
        if (roadRegistryEvents is null)
        {
            return configuration;
        }

        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{WellKnownConnectionNames.Events}"] = roadRegistryEvents
            })
            .Build();
    }

    private IServiceProvider BuildServiceProvider()
    {
        var configuration = BuildConfiguration();
        PrintTargets(configuration);

        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging();

        services
            .AddMartenRoad(options => options
                .AddRoadNetworkTopologyProjection()
                .AddRoadAggregatesSnapshots()).Services
            // Nothing is expected to draw a new id, but if something does it must not collide with the real network.
            .AddRoadNetworkDbIdGenerator();

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }
}
