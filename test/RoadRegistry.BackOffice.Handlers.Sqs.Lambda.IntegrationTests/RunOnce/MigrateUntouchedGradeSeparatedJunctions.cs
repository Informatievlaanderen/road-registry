namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Dapper;
using Marten;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.GradeSeparatedJunction.Changes;
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
/// Before the fix, an inwinning left the grade separated junctions it did not change as V1 junctions: FeatureCompare saw
/// them as identical and produced no change, so they only got GradeSeparatedJunctionGeometryWasChanged while both their
/// road segments were migrated. This migrates those junctions after the fact, the way the inwinning now does:
/// ScopedRoadNetwork.Migrate with a ModifyGradeSeparatedJunctionChange, which appends GradeSeparatedJunctionWasMigrated.
///
/// The junctions are found as the V1 junctions of which both road segments are V2. Their type is the one the inwinning
/// extract contained: the V1 type, translated to V2 the way the extract writer does it.
///
/// This WRITES when <c>dryRun</c> is false: the events land in the configured event store like a real migration. The
/// fix has to be deployed first, otherwise the projections do not know the event. Run it on STG before PRD.
/// </summary>
public class MigrateUntouchedGradeSeparatedJunctions
{
    private const int BatchSize = 100;

    private readonly ITestOutputHelper _outputHelper;

    public MigrateUntouchedGradeSeparatedJunctions(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    //[Fact]
    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // Only lists what it would migrate; set to false to write.
        var dryRun = true;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();
        var repository = new RoadNetworkRepository(store);
        var idGenerator = sp.GetRequiredService<IRoadNetworkIdGenerator>();

        var candidateIds = await FindCandidates(store);
        _outputHelper.WriteLine($"Candidates: {candidateIds.Count}");

        var migrated = new List<GradeSeparatedJunctionId>();
        var skipped = new List<string>();

        foreach (var batch in candidateIds.Chunk(BatchSize))
        {
            await using var session = store.LightweightSession();

            var ids = await repository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [], batch, []));
            var roadNetwork = await repository.Load(session, ids, new ScopedRoadNetworkId(Guid.NewGuid()));
            var extractItems = (await session.LoadManyAsync<GradeSeparatedJunctionExtractItem>(batch.Select(x => x.ToInt32())))
                .ToDictionary(x => x.GradeSeparatedJunctionId);

            var changes = RoadNetworkChanges.Start()
                .WithProvenance(new RoadRegistryProvenanceData(
                    Modification.Update,
                    reason: "Migrate grade separated junctions an inwinning left untouched").ToProvenance());

            foreach (var junctionId in batch)
            {
                var change = TryBuildChange(roadNetwork, extractItems, junctionId, out var reason);
                if (change is null)
                {
                    skipped.Add($"{junctionId}: {reason}");
                    continue;
                }

                _outputHelper.WriteLine($"{junctionId}: lower {change.LowerRoadSegmentId}, upper {change.UpperRoadSegmentId}, {extractItems[junctionId].Type} -> {change.Type}");
                changes.Add(change);
                migrated.Add(junctionId);
            }

            if (dryRun || !changes.Any())
            {
                continue;
            }

            var result = roadNetwork.Migrate(changes, null, idGenerator, NullLogger.Instance);
            if (result.Problems.HasError())
            {
                throw new InvalidOperationException($"Migrating grade separated junctions {string.Join(", ", batch)} failed: {string.Join(", ", result.Problems.Select(x => x.Describe()))}");
            }

            repository.Save(session, roadNetwork, nameof(MigrateUntouchedGradeSeparatedJunctions));
            await session.SaveChangesAsync();
        }

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"{(dryRun ? "Would migrate" : "Migrated")}: {migrated.Count}");
        _outputHelper.WriteLine($"Skipped: {skipped.Count}");
        foreach (var line in skipped)
        {
            _outputHelper.WriteLine($"  {line}");
        }
    }

    // The topology projection is inline, so it is as current as the aggregates. It only preselects: every candidate is
    // judged again on its aggregate before it is migrated.
    private static async Task<IReadOnlyList<GradeSeparatedJunctionId>> FindCandidates(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var ids = await session.Connection!.QueryAsync<int>($@"
SELECT j.id
FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} lower_segment ON lower_segment.id = j.lower_road_segment_id
JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} upper_segment ON upper_segment.id = j.upper_road_segment_id
WHERE NOT j.is_v2 AND lower_segment.is_v2 AND upper_segment.is_v2
ORDER BY j.id");

        return ids.Select(x => new GradeSeparatedJunctionId(x)).ToList();
    }

    private static ModifyGradeSeparatedJunctionChange? TryBuildChange(
        ScopedRoadNetwork roadNetwork,
        IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionExtractItem> extractItems,
        GradeSeparatedJunctionId junctionId,
        out string reason)
    {
        if (!roadNetwork.GradeSeparatedJunctions.TryGetValue(junctionId, out var junction) || junction.IsRemoved)
        {
            reason = "not found or removed";
            return null;
        }

        if (junction.HasMigrated())
        {
            reason = "migrated already";
            return null;
        }

        if (!IsMigratedRoadSegment(roadNetwork, junction.LowerRoadSegmentId) || !IsMigratedRoadSegment(roadNetwork, junction.UpperRoadSegmentId))
        {
            reason = "a road segment is not migrated";
            return null;
        }

        if (!extractItems.TryGetValue(junctionId, out var extractItem) || extractItem.IsV2)
        {
            reason = "no V1 extract item";
            return null;
        }

        var type = MigrateTypeToV2(extractItem.Type);
        if (type is null)
        {
            reason = $"type '{extractItem.Type}' has no V2 counterpart";
            return null;
        }

        reason = string.Empty;
        return new ModifyGradeSeparatedJunctionChange
        {
            GradeSeparatedJunctionId = junctionId,
            LowerRoadSegmentId = junction.LowerRoadSegmentId,
            UpperRoadSegmentId = junction.UpperRoadSegmentId,
            Type = type
        };
    }

    private static bool IsMigratedRoadSegment(ScopedRoadNetwork roadNetwork, RoadSegmentId roadSegmentId)
    {
        return roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment)
               && !roadSegment.IsRemoved
               && roadSegment.HasMigrated();
    }

    // The same translation as the inwinning extract writer (GradeSeparatedJunctionZipArchiveWriter.MigrateToV2), so the
    // junction gets the type the inwinning left untouched.
    private static GradeSeparatedJunctionTypeV2? MigrateTypeToV2(string v1Type)
    {
        if (!GradeSeparatedJunctionType.CanParse(v1Type))
        {
            return null;
        }

        var identifier = GradeSeparatedJunctionType.Parse(v1Type).Translation.Identifier;
        return identifier is 1 or 2 or -8
            ? GradeSeparatedJunctionTypeV2.ByIdentifier[identifier]
            : null;
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
                .AddRoadAggregatesSnapshots()
                .ConfigureExtractDocuments()).Services
            // Nothing is expected to draw a new id, but if something does it must not collide with the real network.
            .AddRoadNetworkDbIdGenerator();

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }
}
