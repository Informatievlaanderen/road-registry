namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Dapper;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Npgsql;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ValueObjects;
using Xunit.Abstractions;

/// <summary>
/// MartenMigrationProjection.RemoveOutlinedRoadSegment marked the aggregate removed on every V1
/// OutlinedRoadSegmentRemoved. That event has two readings, and only one of them means the road segment is gone.
///
/// The old system kept outlined and measured road segments in two SqlStreamStore streams. A road segment whose
/// geometry stopped being outlined moved from the one to the other: its outlined variant was removed while the road
/// segment itself carried on as measured, under the same id. Marten has a single stream per road segment, so that
/// move arrives here as a removal of a road segment that is still very much there - which is why the read, extract,
/// WMS/WFS and PBS projections all ignore this event outright. The aggregate cannot ignore it altogether, because a
/// genuine removal of an outlined road segment arrives on the same event.
///
/// WHICH READING IT IS. The aggregate does not carry the geometry draw method - it lives on RoadSegment.Attributes,
/// which a road segment taken over from the legacy road network does not have until it is migrated - so the stream is
/// what says it. The last V1 event before the removal that carries a draw method (ImportedRoadSegment,
/// RoadSegmentAdded, RoadSegmentModified) says what the road segment was at that moment; a conversion to measured is
/// a RoadSegmentModified with the new draw method, and with convertedFromOutlined on it.
///
/// This REPAIRS the one direction that is a known defect: a document that says isRemoved while the road segment had
/// already stopped being outlined. The other direction - still outlined, removal legitimate, yet the document does
/// not say removed - is only REPORTED. Removing a road segment is not something to do on an inference, and with the
/// old always-remove behaviour that set should be empty; if it is not, that is a finding of its own and wants
/// looking at before anything is written.
///
/// Clearing the flag is a direct update of the document JSON rather than an event applied to the aggregate, because
/// the road segment aggregate has no event that undoes a removal (its road node counterpart does:
/// RoadNodeRemovalWasCorrectedBecauseOfMigration). Nothing is appended to any stream here either - the streams were
/// always right, only the snapshots were not - so no projection sees anything and this can run without a deploy.
/// It is idempotent: a second run finds nothing, because it selects on the difference it removes.
///
/// Run it on STG before PRD. Deploy the RemoveOutlinedRoadSegment fix first, or the migration will keep producing
/// what this cleans up.
/// </summary>
public class RepairRoadSegmentSnapshots
{
    private const int BatchSize = 500;
    private const int MaxDetailLines = 25;

    // Marten's snake_case aliases, kept with the lists in RoadRegistry.Pbs.Compare.Console - whose --eventtypes
    // prints what the store actually holds, which is how these are checked against reality.
    private const string OutlinedRemovedV1 = "outlined_road_segment_removed";
    private const string RemovedV1 = "road_segment_removed";
    private const string RemovedV2 = "road_segment_was_removed";
    private const string RemovedBecauseOfMigration = "road_segment_was_removed_because_of_migration";

    private static readonly string[] OtherRemovals = [RemovedV1, RemovedV2, RemovedBecauseOfMigration];

    private readonly ITestOutputHelper _outputHelper;

    public RepairRoadSegmentSnapshots(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // Only reports what it would rewrite; set to false to write.
        var dryRun = true;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        var candidates = await FindCandidates(store);
        _outputHelper.WriteLine($"Road segments whose state rests on an outlined removal: {candidates.Count}");
        _outputHelper.WriteLine(string.Empty);

        var verdicts = await Judge(store, candidates);

        Report("Wrongly removed - already measured when its outlined variant went away", verdicts, Verdict.WronglyRemoved);
        Report("Rightly removed - still outlined", verdicts, Verdict.RightlyRemoved);
        Report("Undecidable - no draw method anywhere in the stream before the removal", verdicts, Verdict.Undecidable);
        Report("Still outlined and not removed - NOT repaired here, look at these", verdicts, Verdict.NotRemovedWhileOutlined);
        Report("Already right", verdicts, Verdict.AlreadyRight);

        var repaired = await ClearRemovalFlag(store, verdicts.Where(x => x.Outcome == Verdict.WronglyRemoved).Select(x => x.RoadSegmentId).ToList(), dryRun);

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"{(dryRun ? "Would repair" : "Repaired")}: {repaired} road segments");
    }

    private void Report(string title, IReadOnlyList<Judgement> verdicts, Verdict outcome)
    {
        var matching = verdicts.Where(x => x.Outcome == outcome).ToList();
        _outputHelper.WriteLine($"{title}: {matching.Count}");

        foreach (var judgement in matching.Take(MaxDetailLines))
        {
            _outputHelper.WriteLine($"  {judgement.RoadSegmentId}: {judgement.GeometryDrawMethod ?? "<geen>"}");
        }

        if (matching.Count > MaxDetailLines)
        {
            _outputHelper.WriteLine($"  ... and {matching.Count - MaxDetailLines} more");
        }
    }

    /// <summary>
    /// Judges every candidate against its own stream: what the SQL preselected on is the shape of the stream, what
    /// this decides on is the road segment's draw method at the moment of the removal.
    /// </summary>
    private static async Task<IReadOnlyList<Judgement>> Judge(IDocumentStore store, IReadOnlyList<Candidate> candidates)
    {
        var judgements = new List<Judgement>();

        await using var session = store.QuerySession();

        foreach (var candidate in candidates)
        {
            var roadSegmentId = new RoadSegmentId(candidate.RoadSegmentId);
            var streamKey = StreamKeyFactory.Create(typeof(RoadRegistry.RoadSegment.RoadSegment), roadSegmentId);

            var events = (await session.Events.FetchStreamAsync(streamKey))
                .Select(x => x.Data)
                .ToList();

            // Everything up to and including the last outlined removal: what came after it cannot have decided what
            // the road segment was when it arrived.
            var lastOutlinedRemoval = events.FindLastIndex(x => x is RoadRegistry.RoadSegment.Events.V1.OutlinedRoadSegmentRemoved);
            if (lastOutlinedRemoval < 0)
            {
                // The SQL selected on this event being there, so the stream has changed under us.
                continue;
            }

            var geometryDrawMethod = events
                .Take(lastOutlinedRemoval)
                .Select(x => x switch
                {
                    RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment e => e.GeometryDrawMethod,
                    RoadRegistry.RoadSegment.Events.V1.RoadSegmentAdded e => e.GeometryDrawMethod,
                    RoadRegistry.RoadSegment.Events.V1.RoadSegmentModified e => e.GeometryDrawMethod,
                    _ => null
                })
                .LastOrDefault(x => x is not null);

            var wasOutlined = geometryDrawMethod is not null
                              && RoadSegmentGeometryDrawMethod.Parse(geometryDrawMethod) == RoadSegmentGeometryDrawMethod.Outlined;

            var outcome = (candidate.IsRemoved, wasOutlined, geometryDrawMethod) switch
            {
                (true, false, not null) => Verdict.WronglyRemoved,
                (true, false, null) => Verdict.Undecidable,
                (true, true, _) => Verdict.RightlyRemoved,
                (false, true, _) => Verdict.NotRemovedWhileOutlined,
                (false, false, _) => Verdict.AlreadyRight
            };

            judgements.Add(new Judgement(candidate.RoadSegmentId, geometryDrawMethod, outcome));
        }

        return judgements;
    }

    /// <summary>
    /// Clears isRemoved on the document. Straight at the JSON, because the aggregate has no event that undoes a
    /// removal; the generated is_removed column follows the document, so it needs no touching of its own.
    /// </summary>
    private async Task<int> ClearRemovalFlag(IDocumentStore store, IReadOnlyList<int> roadSegmentIds, bool dryRun)
    {
        if (dryRun || roadSegmentIds.Count == 0)
        {
            return roadSegmentIds.Count;
        }

        var repaired = 0;

        foreach (var batch in roadSegmentIds.Chunk(BatchSize))
        {
            await using var session = store.LightweightSession();

            repaired += await session.Connection!.ExecuteAsync($@"
UPDATE {RoadNetworkTopologyTables.RoadSegments}
SET data = jsonb_set(data, '{{isRemoved}}', 'false'::jsonb)
WHERE road_segment_id = ANY(@roadSegmentIds)
  AND is_removed",
                new { roadSegmentIds = batch });
        }

        return repaired;
    }

    /// <summary>
    /// Road segments whose current removed state, if they have one, rests on an outlined removal: the stream holds
    /// one, and no removal of another kind comes after it. Only a preselection - whether that removal was the right
    /// reading is decided against the events themselves.
    /// </summary>
    private static async Task<IReadOnlyList<Candidate>> FindCandidates(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var rows = await session.Connection!.QueryAsync<Candidate>($@"
WITH outlined AS (
    SELECT e.stream_id, max(e.version) AS version
    FROM eventstore.mt_events e
    WHERE e.type = @outlinedRemoved
    GROUP BY e.stream_id
),
other AS (
    SELECT e.stream_id, max(e.version) AS version
    FROM eventstore.mt_events e
    WHERE e.type = ANY(@otherRemovals)
      AND e.stream_id IN (SELECT stream_id FROM outlined)
    GROUP BY e.stream_id
)
SELECT doc.road_segment_id AS {nameof(Candidate.RoadSegmentId)}, doc.is_removed AS {nameof(Candidate.IsRemoved)}
FROM outlined
JOIN {RoadNetworkTopologyTables.RoadSegments} doc ON doc.id = outlined.stream_id
LEFT JOIN other ON other.stream_id = outlined.stream_id
WHERE other.version IS NULL OR other.version < outlined.version
ORDER BY doc.road_segment_id",
            new { outlinedRemoved = OutlinedRemovedV1, otherRemovals = OtherRemovals });

        return rows.AsList();
    }

    private enum Verdict
    {
        WronglyRemoved,
        RightlyRemoved,
        Undecidable,
        NotRemovedWhileOutlined,
        AlreadyRight
    }

    private sealed record Judgement(int RoadSegmentId, string? GeometryDrawMethod, Verdict Outcome);

    private sealed class Candidate
    {
        public int RoadSegmentId { get; init; }
        public bool IsRemoved { get; init; }
    }

    private static IConfiguration BuildConfiguration()
        => new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

    private IServiceProvider BuildServiceProvider()
    {
        var configuration = BuildConfiguration();

        _outputHelper.WriteLine("Target:");
        _outputHelper.WriteLine($"  Marten (aggregates): {DescribePostgres(configuration.GetConnectionString(WellKnownConnectionNames.Marten))}");
        _outputHelper.WriteLine(string.Empty);

        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging();

        services.AddMartenRoad(options => options
            .AddRoadAggregatesSnapshots()
            .ConfigureExtractDocuments());

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
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
}
