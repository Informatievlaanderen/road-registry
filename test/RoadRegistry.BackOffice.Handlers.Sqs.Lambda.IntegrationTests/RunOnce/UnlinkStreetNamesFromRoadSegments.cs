namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using Xunit.Abstractions;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

/// <summary>
/// GAWR-7784. A street name is linked to a road segment only while both are active (see "Beheer koppeling
/// straatnaam-wegsegment"): a road segment now lets go of its street names when it is removed, historized or not
/// realized, and a street name that is removed, retired or rejected is let go by every road segment. This corrects the
/// links that were left behind before that, for the three cases the analysis names:
///
/// - a road segment that is not removed, linked to a street name that is removed, retired (gehistoreerd) or rejected
///   (afgekeurd): only that street name is let go, so a street name on the other side that is still active stays;
/// - a road segment that is not removed but inactive (gehistoreerd or niet gerealiseerd) with a street name: every
///   street name is let go;
/// - a removed road segment with a street name: every street name is let go.
///
/// WHICH ROAD SEGMENTS. Only V2 road segments - the migrated ones, read from the RoadSegment aggregate documents in
/// Marten, which are an inline snapshot and so always up to date. The status of each street name comes from the street
/// name cache; a street name the cache does not know is left alone and only reported.
///
/// HOW. Straight through the domain, the way MigrateUntouchedGradeSeparatedJunctions does: each street name that is let
/// go becomes a RoadSegmentStreetNameWasUnlinked on the road segment's own stream. A removed road segment is no
/// longer in the extract, PBS and WMS/WFS projections, and those skip the event for it; the read projection keeps the
/// removed road segment and follows it.
///
/// ENVIRONMENT. Everything comes from appsettings.runonce.{environment}.json next to the test assembly (git ignores
/// appsettings.*.json): ConnectionStrings:Marten and ConnectionStrings:StreetNameProjections.
///
/// This WRITES when <c>dryRun</c> is false: the events land in the configured event store. Deploy GAWR-7784 first,
/// otherwise the projections do not skip a removed road segment yet, and run it on STG before PRD. It is idempotent: a
/// second run finds nothing to correct.
/// </summary>
public class UnlinkStreetNamesFromRoadSegments
{
    private const int ReadBatchSize = 5000;
    private const int WriteBatchSize = 100;

    private readonly ITestOutputHelper _outputHelper;

    public UnlinkStreetNamesFromRoadSegments(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // tst, stg or prd: picks appsettings.runonce.{environment}.json.
        var environment = "stg";

        // Only lists what it would correct; set to false to write.
        var dryRun = true;

        var configuration = BuildConfiguration(environment);
        var sp = BuildServiceProvider(configuration);
        PrintTargets(environment, configuration, dryRun);

        var store = sp.GetRequiredService<IDocumentStore>();
        var streetNameCache = sp.GetRequiredService<IStreetNameCache>();
        var repository = new RoadNetworkRepository(store);

        var (corrections, unknownStreetNameIds) = await FindCorrections(store, streetNameCache);

        foreach (var correction in corrections)
        {
            _outputHelper.WriteLine($"  {correction.RoadSegmentId}: {correction.Reason}, street names {string.Join(", ", correction.StreetNameIds)}");
        }

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"Road segments to correct: {corrections.Count}");
        foreach (var reason in Enum.GetValues<Reason>())
        {
            _outputHelper.WriteLine($"  {reason}: {corrections.Count(x => x.Reason == reason)}");
        }
        _outputHelper.WriteLine($"Street names unknown to the cache, left alone: {unknownStreetNameIds.Count}{(unknownStreetNameIds.Count > 0 ? $" ({string.Join(", ", unknownStreetNameIds.Order())})" : string.Empty)}");

        if (dryRun || corrections.Count == 0)
        {
            return;
        }

        var provenance = new RoadRegistryProvenanceData(
            Modification.Update,
            reason: "Wegsegmenten ontkoppelen van inactieve straatnamen (GAWR-7784)").ToProvenance();

        var corrected = 0;
        foreach (var batch in corrections.Chunk(WriteBatchSize))
        {
            await using var session = store.LightweightSession();

            // By id, so the removed road segments come along as well: a load by scope leaves those out.
            var roadNetwork = await repository.Load(
                session,
                new RoadNetworkIds([], batch.Select(x => x.RoadSegmentId).ToList(), [], []),
                new ScopedRoadNetworkId(Guid.NewGuid()));

            roadNetwork.UnlinkStreetNames(
                batch.ToDictionary(x => x.RoadSegmentId, x => (IReadOnlyCollection<StreetNameLocalId>)x.StreetNameIds),
                provenance);

            repository.Save(session, roadNetwork, nameof(UnlinkStreetNamesFromRoadSegments));
            await session.SaveChangesAsync();

            corrected += batch.Length;
            _outputHelper.WriteLine($"Corrected {corrected} of {corrections.Count}");
        }
    }

    [Fact]
    public void PlanCorrection_ActiveRoadSegmentWithRetiredStreetName_UnlinksOnlyThatStreetName()
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.Gerealiseerd,
            [new StreetNameLocalId(1), new StreetNameLocalId(2)],
            StreetNames(Item(1, StreetNameStatus.Retired), Item(2, StreetNameStatus.Current)));

        plan.Should().BeEquivalentTo(new Plan(Reason.InactiveStreetName, [new StreetNameLocalId(1)]));
    }

    [Theory]
    [InlineData(StreetNameStatus.Retired)]
    [InlineData(StreetNameStatus.Denied)]
    public void PlanCorrection_ActiveRoadSegmentWithInactiveStreetName_UnlinksIt(string status)
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.Gepland,
            [new StreetNameLocalId(1)],
            StreetNames(Item(1, status)));

        plan.Should().BeEquivalentTo(new Plan(Reason.InactiveStreetName, [new StreetNameLocalId(1)]));
    }

    [Fact]
    public void PlanCorrection_ActiveRoadSegmentWithRemovedStreetName_UnlinksIt()
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.BuitenGebruik,
            [new StreetNameLocalId(1)],
            StreetNames(Item(1, StreetNameStatus.Current) with { IsRemoved = true }));

        plan.Should().BeEquivalentTo(new Plan(Reason.InactiveStreetName, [new StreetNameLocalId(1)]));
    }

    [Theory]
    [InlineData(StreetNameStatus.Proposed)]
    [InlineData(StreetNameStatus.Current)]
    public void PlanCorrection_ActiveRoadSegmentWithActiveStreetName_CorrectsNothing(string status)
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.Gerealiseerd,
            [new StreetNameLocalId(1)],
            StreetNames(Item(1, status)));

        plan.Should().BeNull();
    }

    [Fact]
    public void PlanCorrection_ActiveRoadSegmentWithStreetNameUnknownToTheCache_CorrectsNothing()
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.Gerealiseerd,
            [new StreetNameLocalId(1)],
            StreetNames());

        plan.Should().BeNull();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    public void PlanCorrection_InactiveRoadSegment_UnlinksEveryStreetName(string status)
    {
        var plan = PlanCorrection(
            isRemoved: false,
            RoadSegmentStatusV2.Parse(status),
            [new StreetNameLocalId(1), new StreetNameLocalId(2)],
            StreetNames(Item(1, StreetNameStatus.Current), Item(2, StreetNameStatus.Current)));

        plan.Should().BeEquivalentTo(new Plan(Reason.InactiveRoadSegment, [new StreetNameLocalId(1), new StreetNameLocalId(2)]));
    }

    [Fact]
    public void PlanCorrection_RemovedRoadSegment_UnlinksEveryStreetName()
    {
        var plan = PlanCorrection(
            isRemoved: true,
            RoadSegmentStatusV2.Gerealiseerd,
            [new StreetNameLocalId(1)],
            StreetNames(Item(1, StreetNameStatus.Current)));

        plan.Should().BeEquivalentTo(new Plan(Reason.RemovedRoadSegment, [new StreetNameLocalId(1)]));
    }

    [Fact]
    public void PlanCorrection_NoStreetName_CorrectsNothing()
    {
        var plan = PlanCorrection(
            isRemoved: true,
            RoadSegmentStatusV2.Gehistoreerd,
            [StreetNameLocalId.NotApplicable, StreetNameLocalId.Unknown],
            StreetNames());

        plan.Should().BeNull();
    }

    private async Task<(IReadOnlyList<Correction> Corrections, IReadOnlyCollection<int> UnknownStreetNameIds)> FindCorrections(IDocumentStore store, IStreetNameCache streetNameCache)
    {
        var corrections = new List<Correction>();
        var unknownStreetNameIds = new HashSet<int>();
        var lastId = 0;
        var read = 0;

        while (true)
        {
            // The generated columns of the aggregate document (migration 20261103120000): has_migrated is
            // RoadSegment.HasMigrated. The removed road segments are read as well.
            await using var session = store.QuerySession();
            var roadSegments = (await session.QueryAsync<RoadSegment>(
                    "where road_segment_id > ? and has_migrated order by road_segment_id limit ?",
                    lastId, ReadBatchSize))
                .ToList();
            if (roadSegments.Count == 0)
            {
                break;
            }

            lastId = roadSegments.Max(x => x.RoadSegmentId.ToInt32());
            read += roadSegments.Count;

            var linked = roadSegments
                .Where(x => x.HasMigrated())
                .Select(x => (RoadSegment: x, StreetNameIds: LinkedStreetNameIds(x)))
                .Where(x => x.StreetNameIds.Count > 0)
                .ToList();

            var streetNameIds = linked
                .SelectMany(x => x.StreetNameIds)
                .Select(x => x.ToInt32())
                .Distinct()
                .ToList();
            var streetNames = (await streetNameCache.GetAsync(streetNameIds, CancellationToken.None))
                .ToDictionary(x => x.Id);
            unknownStreetNameIds.UnionWith(streetNameIds.Where(id => !streetNames.ContainsKey(id)));

            foreach (var (roadSegment, linkedStreetNameIds) in linked)
            {
                var plan = PlanCorrection(roadSegment.IsRemoved, roadSegment.Status, linkedStreetNameIds, streetNames);
                if (plan is not null)
                {
                    corrections.Add(new Correction(roadSegment.RoadSegmentId, plan.Reason, plan.StreetNameIds));
                }
            }

            _outputHelper.WriteLine($"Read {read} migrated road segments, {corrections.Count} to correct so far");
        }

        return (corrections, unknownStreetNameIds);
    }

    private static IReadOnlyList<StreetNameLocalId> LinkedStreetNameIds(RoadSegment roadSegment)
    {
        return roadSegment.Attributes!.StreetNameId.Values
            .Select(x => x.Value)
            .Where(x => !StreetNameLocalId.IsEmpty(x))
            .Distinct()
            .ToList();
    }

    // The street names to let go of, and why, or null when the road segment keeps them all.
    private static Plan? PlanCorrection(
        bool isRemoved,
        RoadSegmentStatusV2 status,
        IReadOnlyCollection<StreetNameLocalId> linkedStreetNameIds,
        IReadOnlyDictionary<int, StreetNameCacheItem> streetNames)
    {
        var linked = linkedStreetNameIds.Where(x => !StreetNameLocalId.IsEmpty(x)).Distinct().ToList();
        if (linked.Count == 0)
        {
            return null;
        }

        if (isRemoved)
        {
            return new Plan(Reason.RemovedRoadSegment, linked);
        }

        if (!status.IsActive)
        {
            return new Plan(Reason.InactiveRoadSegment, linked);
        }

        var inactive = linked
            .Where(id => streetNames.TryGetValue(id.ToInt32(), out var streetName) && IsInactive(streetName))
            .ToList();
        return inactive.Count > 0
            ? new Plan(Reason.InactiveStreetName, inactive)
            : null;
    }

    private static bool IsInactive(StreetNameCacheItem streetName)
    {
        return streetName.IsRemoved
               || string.Equals(streetName.Status, StreetNameStatus.Retired, StringComparison.InvariantCultureIgnoreCase)
               || string.Equals(streetName.Status, StreetNameStatus.Denied, StringComparison.InvariantCultureIgnoreCase);
    }

    private static StreetNameCacheItem Item(int id, string status)
    {
        return new StreetNameCacheItem { Id = id, NisCode = "11001", Name = $"Straat {id}", Status = status };
    }

    private static IReadOnlyDictionary<int, StreetNameCacheItem> StreetNames(params StreetNameCacheItem[] items)
    {
        return items.ToDictionary(x => x.Id);
    }

    private void PrintTargets(string environment, IConfiguration configuration, bool dryRun)
    {
        _outputHelper.WriteLine($"Environment: {environment}{(dryRun ? " (dry run)" : string.Empty)}");
        var marten = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString(WellKnownConnectionNames.Marten));
        _outputHelper.WriteLine($"  Marten: {marten.Host}:{marten.Port}/{marten.Database}");
        _outputHelper.WriteLine(string.Empty);
    }

    private static IConfiguration BuildConfiguration(string environment)
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile($"appsettings.runonce.{environment}.json", optional: false, reloadOnChange: false)
            .Build();
    }

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging();

        services
            .AddMartenRoad(options => options.AddRoadAggregatesSnapshots()).Services
            .AddStreetNameCache();

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    private enum Reason
    {
        InactiveStreetName,
        InactiveRoadSegment,
        RemovedRoadSegment
    }

    private sealed record Plan(Reason Reason, IReadOnlyList<StreetNameLocalId> StreetNameIds);

    private sealed record Correction(RoadSegmentId RoadSegmentId, Reason Reason, IReadOnlyList<StreetNameLocalId> StreetNameIds);
}
