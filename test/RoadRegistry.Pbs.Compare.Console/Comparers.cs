namespace RoadRegistry.Pbs.Compare.Console;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ValueObjects;

// Two sources, each for what it has authority over.
//
// WHETHER an entity should exist comes from the event streams: per stream the last event that creates or removes it
// wins (see Existences). The document's own isRemoved is the second opinion, never the verdict: the two are held
// against each other and whatever drift is left over is reported as a note.
//
// WHAT it should look like is read from the aggregate documents in {EventStoreSchema}.mt_doc_*. The snapshot is
// written inline, in the same transaction that appends the events, so it is never behind the event store. What we
// compare on - the id, the status, the node ids, the geometry - sits in columns generated from the document json by
// 20261103120000_add_topology_columns_to_aggregate_documents, which Postgres keeps in step with every upsert Marten
// does. The networktopology_* tables this tool used to read are no longer projected into (1a813da62).
//
// The month a row is bucketed by comes from the event stream too: a document carries no event timestamp of its own,
// and mt_last_modified says when the snapshot was written, not when the change happened.
//
// What each side is asked for is deliberately shallow: does the row exist, is it gone when it should be gone, and do
// the few fields that change on almost every edit still line up. This finds events PBS never applied; it is not a
// value-by-value audit of the attribute tables.
internal static class Comparers
{
    private const string PbsSchema = WellKnownSchemas.PbsSchema;
    private static readonly string EventStoreSchema = WellKnownSchemas.MartenEventStore;

    // What the event streams say about one entity: whether it still exists, and the month of the event that decided
    // that. Kept as yyyyMM rather than a DateTimeOffset because one of these is held per id of the whole network.
    internal readonly record struct EntityExistence(bool Removed, int YearMonth);

    internal sealed record SegmentRow(bool Removed, bool DocumentRemoved, bool HasMigrated, int? StartNode, int? EndNode, int? Status, string? StatusText, int Points, double Length, int YearMonth);

    internal sealed record NodeRow(bool Removed, bool DocumentRemoved, bool HasMigrated, double X, double Y, int YearMonth);

    internal sealed record JunctionRow(bool Removed, bool DocumentRemoved, bool HasMigrated, int? SegmentA, int? SegmentB, int YearMonth);

    // Why a row is in the state it is in, counted over every finding. This is what turns "33.234 ontbreken" into a
    // diagnosis: a row still carried over from the legacy road network - no attributes on the document yet, so
    // nothing has migrated it - is a different story from one that a merge or split retired.
    private static string Category(SegmentRow row) => $"{Migrated(row.HasMigrated)}, status={row.StatusText ?? "-"}";

    private static string Category(NodeRow row) => Migrated(row.HasMigrated);

    private static string Category(JunctionRow row) => Migrated(row.HasMigrated);

    private static string Migrated(bool hasMigrated) => hasMigrated ? "gemigreerd" : "niet-gemigreerd";

    // The month of the last event that decided whether the entity exists: a gap that clusters in one window is an
    // incident, a gap that is spread evenly is something systematic.
    private static string Month(int yearMonth)
        => yearMonth == 0
            ? "-"
            : string.Create(CultureInfo.InvariantCulture, $"{yearMonth / 100:D4}-{yearMonth % 100:D2}");

    private static int YearMonth(DateTimeOffset timestamp)
        => timestamp == default ? 0 : timestamp.Year * 100 + timestamp.Month;

    // Read-only diagnosis of the one way this driver loses work: RoadNetworkChangesProjection.IsProjectable drops
    // every event without a correlation id, while the projection-state position still advances past it
    // ("it is never applied and never will be, since the progression moves on regardless"). If such events exist for
    // road network types, they are exactly the entities that never reached the read model.
    public static async Task DiagnoseAsync(DbConnection marten, CancellationToken ct)
    {
        await using (var totals = marten.CreateCommand())
        {
            totals.CommandText = $"""
                select count(*) filter (where correlation_id is null) as zonder,
                       count(*) as totaal
                from {EventStoreSchema}.mt_events
                """;
            totals.CommandTimeout = 0;
            await using var reader = await totals.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                Console.WriteLine($"Events zonder correlation id: {reader.GetInt64(0):N0} van {reader.GetInt64(1):N0}.");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Per eventtype (interne mt_-streams uitgesloten, die horen er geen te hebben):");
        await using var byType = marten.CreateCommand();
        byType.CommandText = $"""
            select type, count(*) as aantal, min(timestamp)::date as van, max(timestamp)::date as tot
            from {EventStoreSchema}.mt_events
            where correlation_id is null and stream_id not like 'mt\_%'
            group by type
            order by count(*) desc
            limit 30
            """;
        byType.CommandTimeout = 0;
        await using var rows = await byType.ExecuteReaderAsync(ct);
        var any = false;
        while (await rows.ReadAsync(ct))
        {
            any = true;
            Console.WriteLine($"  {rows.GetString(0),-50} {rows.GetInt64(1),12:N0}   {rows.GetFieldValue<DateTime>(2):yyyy-MM-dd} .. {rows.GetFieldValue<DateTime>(3):yyyy-MM-dd}");
        }

        if (!any)
        {
            Console.WriteLine("  geen - elk event buiten de interne streams heeft een correlation id");
        }
    }

    // Dumps the event types on the given streams, in order. Used to hold a missing entity's history next to a
    // present one's: if they differ by event type, the gap is about which events this projection ever sees.
    public static async Task DumpStreamsAsync(DbConnection marten, IReadOnlyList<string> streamKeys, CancellationToken ct)
    {
        foreach (var streamKey in streamKeys)
        {
            Console.WriteLine();
            Console.WriteLine($"{streamKey}:");
            await using var command = marten.CreateCommand();
            command.CommandText = $"""
                select seq_id, type, timestamp::date, correlation_id is null as zonder_correlatie
                from {EventStoreSchema}.mt_events
                where stream_id = @stream
                order by seq_id
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "stream";
            parameter.Value = streamKey;
            command.Parameters.Add(parameter);

            await using var reader = await command.ExecuteReaderAsync(ct);
            var any = false;
            while (await reader.ReadAsync(ct))
            {
                any = true;
                var zonder = reader.GetBoolean(3) ? "  GEEN CORRELATION ID" : string.Empty;
                Console.WriteLine($"  {reader.GetInt64(0),12}  {reader.GetString(1),-45} {reader.GetFieldValue<DateTime>(2):yyyy-MM-dd}{zonder}");
            }

            if (!any)
            {
                Console.WriteLine("  geen events op deze stream");
            }
        }
    }

    // The one-off that pins down how Marten spells the event types this tool has to recognise: the removal sets are
    // built from .NET type names and converted by convention, and this prints what the store actually holds so the
    // conversion is checked against it rather than assumed.
    public static async Task DumpEventTypesAsync(DbConnection marten, CancellationToken ct)
    {
        await using var command = marten.CreateCommand();
        command.CommandText = $"""
            select type, count(*) as aantal
            from {EventStoreSchema}.mt_events
            group by type
            order by type
            """;
        command.CommandTimeout = 0;
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Console.WriteLine($"  {reader.GetString(0),-62} {reader.GetInt64(1),12:N0}");
        }
    }

    // The aggregate's own verdict on a road segment: the snapshot is written inline, after every event of the
    // stream, so isRemoved there is the domain speaking rather than anything derived from it.
    public static async Task DumpDocumentsAsync(DbConnection marten, IReadOnlyList<string> ids, CancellationToken ct)
    {
        Console.WriteLine($"Documenten in {EventStoreSchema}.mt_doc_roadsegment: "
                          + $"{Convert.ToInt64(await ScalarAsync(marten, $"select count(*) from {EventStoreSchema}.mt_doc_roadsegment", ct), CultureInfo.InvariantCulture):N0}, "
                          + $"waarvan verwijderd: {Convert.ToInt64(await ScalarAsync(marten, $"select count(*) from {EventStoreSchema}.mt_doc_roadsegment where (data ->> 'isRemoved')::boolean", ct), CultureInfo.InvariantCulture):N0}.");
        Console.WriteLine();

        await using var command = marten.CreateCommand();
        command.CommandText = $"""
            select doc.id as stream_key,
                   (doc.data ->> 'roadSegmentId')::int as id,
                   coalesce(doc.data ->> 'isRemoved', '-') as is_removed,
                   coalesce(doc.data ->> 'status', '-') as status,
                   coalesce(doc.data ->> 'geometryDrawMethod', '-') as draw_method,
                   doc.mt_last_modified
                   -- draw_method blijft in de select staan zodat de kolomindexen hierboven kloppen
            from {EventStoreSchema}.mt_doc_roadsegment doc
            where (doc.data ->> 'roadSegmentId')::int = any(string_to_array(@ids, ',')::int[])
            order by id
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "ids";
        parameter.Value = string.Join(",", ids);
        command.Parameters.Add(parameter);

        Console.WriteLine($"{"streamkey",28} {"id",12} {"isRemoved",10} {"status",-16} laatst gewijzigd");
        await using var reader = await command.ExecuteReaderAsync(ct);
        var found = 0;
        while (await reader.ReadAsync(ct))
        {
            found++;
            Console.WriteLine($"{reader.GetString(0),28} {reader.GetInt32(1),12} {reader.GetString(2),10} {reader.GetString(3),-16} {reader.GetFieldValue<DateTimeOffset>(5):yyyy-MM-dd HH:mm}");
        }

        Console.WriteLine($"({found} van {ids.Count} gevraagde id's hebben een document)");
    }

    // The one independent witness in reach: the EditorContext is projected from SqlStreamStore's V1 stream by other
    // code, and it sits in the same SQL Server as PBS. When PBS and the aggregate disagree about whether a V1 road
    // segment still exists, this is what breaks the tie.
    public static async Task DumpEditorAsync(DbConnection pbs, IReadOnlyList<string> ids, CancellationToken ct)
    {
        const string editorSchema = WellKnownSchemas.EditorSchema;

        Console.WriteLine($"Wegsegmenten in [{editorSchema}].[RoadSegment]: "
                          + $"{Convert.ToInt64(await ScalarAsync(pbs, $"select count(*) from [{editorSchema}].[RoadSegment]", ct), CultureInfo.InvariantCulture):N0}.");
        Console.WriteLine();

        await using var command = pbs.CreateCommand();
        command.CommandText = $"""
            select value as id,
                   case when e.[Id] is null then 'nee' else 'ja' end as in_editor,
                   case when p.[WS_OIDN] is null then 'nee' else 'ja' end as in_pbs,
                   coalesce(e.[MethodId], -1) as method_id,
                   coalesce(e.[StatusId], -1) as status_id
            from string_split(@ids, ',')
            left join [{editorSchema}].[RoadSegment] e on e.[Id] = cast(value as int)
            left join [{PbsSchema}].[Wegsegmenten] p on p.[WS_OIDN] = cast(value as int)
            order by cast(value as int)
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "ids";
        parameter.Value = string.Join(",", ids);
        command.Parameters.Add(parameter);

        Console.WriteLine($"{"id",12} {"in editor",10} {"in pbs",8} {"methode",8} {"status",8}");
        var inEditor = 0;
        var total = 0;
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            total++;
            var editor = reader.GetString(1);
            if (editor == "ja")
            {
                inEditor++;
            }

            Console.WriteLine($"{reader.GetString(0),12} {editor,10} {reader.GetString(2),8} {reader.GetInt32(3),8} {reader.GetInt32(4),8}");
        }

        Console.WriteLine();
        Console.WriteLine($"{inEditor} van {total} id's staan nog in de editor-projectie.");
    }

    // Prints the generated topology columns of a road segment's document, to hold against that stream's events.
    public static async Task DumpTopologyAsync(DbConnection marten, IReadOnlyList<string> ids, CancellationToken ct)
    {
        await using var command = marten.CreateCommand();
        command.CommandText = $"""
            select road_segment_id, coalesce(has_migrated, false), is_removed,
                   coalesce(status, '-') as status, mt_last_modified
            from {RoadNetworkTopologyTables.RoadSegments}
            where road_segment_id = any(string_to_array(@ids, ',')::int[])
            order by road_segment_id
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "ids";
        parameter.Value = string.Join(",", ids);
        command.Parameters.Add(parameter);

        Console.WriteLine($"{"id",12} {"gemigr.",8} {"is_removed",11} {"status",-16} laatst gewijzigd");
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Console.WriteLine($"{reader.GetInt32(0),12} {reader.GetBoolean(1),8} {reader.GetBoolean(2),11} {reader.GetString(3),-16} {reader.GetFieldValue<DateTimeOffset>(4):yyyy-MM-dd HH:mm}");
        }
    }

    // Whether an entity should exist is decided by its event stream, not by the isRemoved on its document. The two
    // normally agree - the snapshot is written inline, after every event of the stream - but the stream is the one
    // that cannot have been skipped, and any disagreement between them is itself worth reporting.
    //
    // So per stream the last event that creates or removes the entity wins, and everything between them is a
    // modification that does not move existence. The aliases are Marten's snake_case convention; --eventtypes
    // prints what the store actually holds, which is how this list is kept honest.
    internal sealed record Existence(string Kind, string[] Creates, string[] Removes)
    {
        public IEnumerable<string> All => Creates.Concat(Removes);
    }

    public const string SegmentKind = "RoadSegment";
    public const string NodeKind = "RoadNode";
    public const string GradeSeparatedJunctionKind = "GradeSeparatedJunction";
    public const string GradeJunctionKind = "GradeJunction";

    private static readonly Existence[] Existences =
    [
        new(SegmentKind,
            [
                "imported_road_segment",
                "road_segment_added",
                "road_segment_was_added",
                "outlined_road_segment_was_added",
                "road_segment_was_migrated"
                // road_segment_was_merged and road_segment_was_split are deliberately in neither list. They are
                // appended to the stream of the segment that was merged or split away, right behind its
                // ..._was_retired_because_of_... - counting them as a create would undo that removal. On the stream
                // of a segment they produce they are the first event, and a stream with no existence event at all
                // is taken as existing, so leaving them out is right on both ends.
            ],
            [
                "road_segment_removed",
                // outlined_road_segment_removed is NOT a removal. In the old system it moved a road segment to
                // another SqlStreamStore stream once its geometry stopped being outlined: the outlined variant went,
                // the measured road segment stayed. The read and extract projections and the network topology all
                // ignore it; PBS and WmsWfsV2 removed the row on it, which is what this tool turned up.
                "road_segment_was_removed",
                "road_segment_was_removed_because_of_migration",
                // The inputs of a merge or a split: the topology keeps them as gehistoreerd, PBS removes them.
                "road_segment_was_retired_because_of_merger",
                "road_segment_was_retired_because_of_split"
            ]),
        new(NodeKind,
            [
                "imported_road_node",
                "road_node_added",
                "road_node_was_added",
                "road_node_was_migrated",
                // The domain brings the node back, so from here on it should exist again.
                "road_node_removal_was_corrected_because_of_migration"
            ],
            [
                "road_node_removed",
                "road_node_was_removed",
                "road_node_was_removed_because_of_migration"
            ]),
        new(GradeSeparatedJunctionKind,
            [
                "imported_grade_separated_junction",
                "grade_separated_junction_added",
                "grade_separated_junction_was_added",
                "grade_separated_junction_was_added_because_of_grade_junction_change",
                "grade_separated_junction_was_migrated"
            ],
            [
                "grade_separated_junction_removed",
                "grade_separated_junction_was_removed",
                "grade_separated_junction_was_removed_because_of_migration",
                // It lives on as a grade junction, under its own id, so this id is gone.
                "grade_separated_junction_was_changed_to_grade_junction"
            ]),
        new(GradeJunctionKind,
            [
                "grade_junction_was_added",
                "grade_junction_was_added_because_of_grade_separated_junction_change"
            ],
            [
                "grade_junction_was_removed",
                "grade_junction_was_changed_to_grade_separated_junction"
            ])
    ];

    /// <summary>
    /// Per entity kind, what the event streams say about every id: whether the last event that moves existence
    /// removes it, and the month that event carries. One pass over the event store for all four kinds together:
    /// the type names do not overlap between them, so the stream key says which kind a row belongs to.
    /// </summary>
    /// <remarks>
    /// Every id is kept, not only the removed ones, because the month a finding is bucketed by comes from here as
    /// well - the document has no event timestamp of its own.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<int, EntityExistence>>> ReadExistenceAsync(
        DbConnection marten, Report report, CancellationToken ct)
    {
        var existence = Existences.ToDictionary(e => e.Kind, _ => new Dictionary<int, EntityExistence>());
        var removals = Existences.SelectMany(e => e.Removes).ToHashSet(StringComparer.Ordinal);
        var perType = new Dictionary<string, int>();

        await using var command = marten.CreateCommand();
        command.CommandText = $"""
            select split_part(last.stream_id, '-', 1) as kind,
                   split_part(last.stream_id, '-', 2)::int as id,
                   last.type,
                   last.timestamp
            from (
                select distinct on (e.stream_id) e.stream_id, e.type, e.timestamp
                from {EventStoreSchema}.mt_events e
                where e.type = any(@types)
                order by e.stream_id, e.seq_id desc
            ) last
            """;
        command.CommandTimeout = 0;
        AddArray(command, "types", Existences.SelectMany(e => e.All).ToArray());

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var kind = reader.GetString(0);
            if (!existence.TryGetValue(kind, out var rows))
            {
                continue;
            }

            var type = reader.GetString(2);
            var removed = removals.Contains(type);
            rows[reader.GetInt32(1)] = new EntityExistence(removed, YearMonth(reader.GetFieldValue<DateTimeOffset>(3)));

            if (removed)
            {
                perType[type] = perType.GetValueOrDefault(type) + 1;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Volgens de events verwijderd (laatste bestaansevent per stream)");
        foreach (var (type, count) in perType.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {type,-54} {count,12:N0}");
        }

        foreach (var kind in existence.Keys)
        {
            var rows = existence[kind];
            Console.WriteLine($"  {kind,-54} {rows.Values.Count(row => row.Removed),12:N0} id's van {rows.Count:N0}");
        }

        if (perType.Count == 0)
        {
            report.Note("geen enkel verwijderevent gevonden - de eventtypes in deze tool kloppen niet meer met de store, "
                        + "voer --eventtypes uit en vergelijk");
        }

        return existence.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyDictionary<int, EntityExistence>)pair.Value);
    }

    private static void AddArray(DbCommand command, string name, string[] values)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = values;
        command.Parameters.Add(parameter);
    }

    public static async Task ComparePositionsAsync(DbConnection marten, DbConnection pbs, Report report, CancellationToken ct)
    {
        var head = Convert.ToInt64(
            await ScalarAsync(marten, $"select coalesce(max(seq_id), 0) from {EventStoreSchema}.mt_events", ct),
            CultureInfo.InvariantCulture);

        Console.WriteLine($"Marten event store staat op sequentie {head:N0}.");

        await using var command = pbs.CreateCommand();
        command.CommandText = $"select [Name], [Position] from [{PbsSchema}].[ProjectionStates] order by [Name]";
        await using var reader = await command.ExecuteReaderAsync(ct);

        var any = false;
        while (await reader.ReadAsync(ct))
        {
            any = true;
            var name = reader.GetString(0);
            var position = reader.GetInt64(1);
            var behind = head - position;
            Console.WriteLine(behind <= 0
                ? $"  {name}: positie {position:N0} - bij"
                : $"  {name}: positie {position:N0} - {behind:N0} events achter");

            if (behind > 0)
            {
                report.Note($"{name} loopt {behind:N0} events achter; verschillen hieronder kunnen gewoon achterstand zijn.");
            }
        }

        if (!any)
        {
            report.Note($"[{PbsSchema}].[ProjectionStates] is leeg - de PBS-projectie heeft nog nooit iets verwerkt.");
        }
    }

    public static async Task CompareSegmentsAsync(DbConnection marten, DbConnection pbs, CompareOptions options,
        IReadOnlyDictionary<int, EntityExistence> existence, Report report, CancellationToken ct)
    {
        const string entity = "wegsegment";
        var unparseableStatus = 0;
        long martenCount = 0, pbsCount = 0, compared = 0, unmarkedRemovals = 0, unexpectedMarks = 0;

        var martenRows = ReadAsync(marten, $"""
            select road_segment_id, is_removed, status, start_node_id, end_node_id,
                   coalesce(st_npoints(geometry), 0), coalesce(st_length(geometry), 0),
                   coalesce(has_migrated, false)
            from {RoadNetworkTopologyTables.RoadSegments}
            order by road_segment_id
            """,
            reader =>
            {
                var id = reader.GetInt32(0);
                var events = existence.GetValueOrDefault(id);
                int? status = null;
                var hasMigrated = reader.GetBoolean(7);
                var statusText = reader.IsDBNull(2) ? null : reader.GetString(2);
                if (statusText is not null)
                {
                    status = ResolveStatus(statusText);
                    if (status is null && hasMigrated)
                    {
                        unparseableStatus++;
                    }
                }

                return new SegmentRow(
                    events.Removed,
                    reader.GetBoolean(1),
                    hasMigrated,
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    status,
                    statusText,
                    reader.GetInt32(5),
                    reader.GetDouble(6),
                    events.YearMonth);
            }, ct);

        var pbsRows = ReadAsync(pbs, $"""
            select [WS_OIDN], [B_WK_OIDN], [E_WK_OIDN], [STATUS],
                   coalesce([GEOMETRIE].STNumPoints(), 0), coalesce([GEOMETRIE].STLength(), 0)
            from [{PbsSchema}].[Wegsegmenten]
            order by [WS_OIDN]
            """,
            reader => new SegmentRow(
                false,
                false,
                false,
                reader.IsDBNull(1) ? null : reader.GetInt32(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                null,
                reader.GetInt32(4),
                reader.GetDouble(5),
                0), ct);

        await foreach (var (id, left, right) in MergeJoin(martenRows, pbsRows, ct))
        {
            if (left is not null)
            {
                martenCount++;
            }

            if (right is not null)
            {
                pbsCount++;
            }

            if (left is not null && left.Removed != left.DocumentRemoved)
            {
                if (left.Removed)
                {
                    unmarkedRemovals++;
                }
                else
                {
                    unexpectedMarks++;
                }
            }

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.YearMonth), right is not null);
            }

            switch (left, right)
            {
                case ({ Removed: false }, null):
                    report.Add(entity, Finding.MissingInPbs, id,
                        $"marten {Category(left!)} knopen={Show(left.StartNode)}->{Show(left.EndNode)}", Category(left));
                    break;
                case ({ Removed: true }, not null):
                    report.Add(entity, Finding.StillInPbsAfterRemoval, id, string.Empty, Category(left!));
                    break;
                case (null, not null):
                    report.Add(entity, Finding.OnlyInPbs, id, $"pbs status={Show(right!.Status)}");
                    break;
                case ({ Removed: false }, not null):
                    compared++;
                    var differences = DescribeSegmentDifferences(left!, right!, options.LengthTolerance);
                    if (differences is not null)
                    {
                        report.Add(entity, Finding.StaleInPbs, id, differences, Category(left!));
                    }

                    break;
            }
        }

        report.Scanned(entity, martenCount, pbsCount, compared);
        NoteDocumentDrift(report, entity, unmarkedRemovals, unexpectedMarks);
        if (unparseableStatus > 0)
        {
            report.Note($"{unparseableStatus:N0} wegsegmenten hadden een status die deze tool niet kon lezen; status is voor die rijen niet vergeleken.");
        }
    }

    public static async Task CompareNodesAsync(DbConnection marten, DbConnection pbs,
        IReadOnlyDictionary<int, EntityExistence> existence, Report report, CancellationToken ct)
    {
        const string entity = "wegknoop";
        long martenCount = 0, pbsCount = 0, compared = 0, unmarkedRemovals = 0, unexpectedMarks = 0;

        // 20261103120000 gave the road node document no geometry column - no lookup needed one - so the point is
        // read straight from the json here, in the same shape that migration builds the road segment's from.
        //
        // has_migrated is "jsonb_typeof(data -> ...) = ...", which is NULL rather than false when the document has
        // no such key at all - and that is precisely the row that has not migrated yet, so every read of it here
        // coalesces. (The same holds for the generated columns on the other three documents.)
        var martenRows = ReadAsync(marten, $"""
            with node as (
                select road_node_id,
                       is_removed,
                       coalesce(jsonb_typeof(data -> 'type') = 'string', false) as has_migrated,
                       public.st_geomfromtext(
                           nullif(data -> 'geometry' ->> 'wkt', ''),
                           (data -> 'geometry' ->> 'srid')::int) as geometry
                from {RoadNetworkTopologyTables.RoadNodes}
            )
            select road_node_id, is_removed, coalesce(st_x(geometry), 0), coalesce(st_y(geometry), 0), has_migrated
            from node
            order by road_node_id
            """,
            reader =>
            {
                var events = existence.GetValueOrDefault(reader.GetInt32(0));
                return new NodeRow(events.Removed, reader.GetBoolean(1), reader.GetBoolean(4),
                    reader.GetDouble(2), reader.GetDouble(3), events.YearMonth);
            }, ct);

        var pbsRows = ReadAsync(pbs, $"""
            select [WK_OIDN], coalesce([GEOMETRIE].STX, 0), coalesce([GEOMETRIE].STY, 0)
            from [{PbsSchema}].[WegknopenData]
            order by [WK_OIDN]
            """,
            reader => new NodeRow(false, false, false, reader.GetDouble(1), reader.GetDouble(2), 0), ct);

        await foreach (var (id, left, right) in MergeJoin(martenRows, pbsRows, ct))
        {
            if (left is not null)
            {
                martenCount++;
            }

            if (right is not null)
            {
                pbsCount++;
            }

            if (left is not null && left.Removed != left.DocumentRemoved)
            {
                if (left.Removed)
                {
                    unmarkedRemovals++;
                }
                else
                {
                    unexpectedMarks++;
                }
            }

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.YearMonth), right is not null);
            }

            switch (left, right)
            {
                case ({ Removed: false }, null):
                    report.Add(entity, Finding.MissingInPbs, id, $"marten {Category(left!)}", Category(left));
                    break;
                case ({ Removed: true }, not null):
                    report.Add(entity, Finding.StillInPbsAfterRemoval, id, string.Empty, Category(left!));
                    break;
                case (null, not null):
                    report.Add(entity, Finding.OnlyInPbs, id);
                    break;
                case ({ Removed: false }, not null):
                    compared++;
                    // Both sides hold the same centimetre-rounded coordinate, so anything above a centimetre is a
                    // geometry PBS never updated.
                    if (Math.Abs(left!.X - right!.X) > 0.01 || Math.Abs(left.Y - right.Y) > 0.01)
                    {
                        report.Add(entity, Finding.StaleInPbs, id,
                            $"marten=({Show(left.X)} {Show(left.Y)}) pbs=({Show(right.X)} {Show(right.Y)})", Category(left));
                    }

                    break;
            }
        }

        report.Scanned(entity, martenCount, pbsCount, compared);
        NoteDocumentDrift(report, entity, unmarkedRemovals, unexpectedMarks);
    }

    public static Task CompareGradeSeparatedJunctionsAsync(DbConnection marten, DbConnection pbs,
        IReadOnlyDictionary<int, EntityExistence> existence, Report report, CancellationToken ct)
        => CompareJunctionsAsync(
            marten, pbs, existence, report, ct,
            entity: "ongelijkgrondse kruising",
            // BO = bovenliggend = upper, ON = onderliggend = lower.
            martenSql: $"""
                select grade_separated_junction_id, is_removed, upper_road_segment_id, lower_road_segment_id,
                       coalesce(has_migrated, false) as has_migrated
                from {RoadNetworkTopologyTables.GradeSeparatedJunctions}
                order by grade_separated_junction_id
                """,
            pbsSql: $"""
                select [OK_OIDN], [BO_WS_OIDN], [ON_WS_OIDN]
                from [{PbsSchema}].[OngelijkgrondseKruisingen]
                order by [OK_OIDN]
                """,
            labelA: "boven", labelB: "onder");

    public static Task CompareGradeJunctionsAsync(DbConnection marten, DbConnection pbs,
        IReadOnlyDictionary<int, EntityExistence> existence, Report report, CancellationToken ct)
        => CompareJunctionsAsync(
            marten, pbs, existence, report, ct,
            entity: "gelijkgrondse kruising",
            // Grade junctions only exist in the V2 road network, so there is nothing to migrate and no column for it.
            martenSql: $"""
                select grade_junction_id, is_removed, road_segment_id_1, road_segment_id_2, true as has_migrated
                from {RoadNetworkTopologyTables.GradeJunctions}
                order by grade_junction_id
                """,
            pbsSql: $"""
                select [GK_OIDN], [WS1_OIDN], [WS2_OIDN]
                from [{PbsSchema}].[GelijkgrondseKruisingen]
                order by [GK_OIDN]
                """,
            labelA: "ws1", labelB: "ws2");

    private static async Task CompareJunctionsAsync(
        DbConnection marten, DbConnection pbs, IReadOnlyDictionary<int, EntityExistence> existence,
        Report report, CancellationToken ct,
        string entity, string martenSql, string pbsSql, string labelA, string labelB)
    {
        long martenCount = 0, pbsCount = 0, compared = 0, unmarkedRemovals = 0, unexpectedMarks = 0;

        var martenRows = ReadAsync(marten, martenSql,
            reader =>
            {
                var events = existence.GetValueOrDefault(reader.GetInt32(0));
                return new JunctionRow(
                    events.Removed,
                    reader.GetBoolean(1),
                    reader.GetBoolean(4),
                    reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    events.YearMonth);
            }, ct);

        var pbsRows = ReadAsync(pbs, pbsSql,
            reader => new JunctionRow(
                false,
                false,
                false,
                reader.IsDBNull(1) ? null : reader.GetInt32(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                0), ct);

        await foreach (var (id, left, right) in MergeJoin(martenRows, pbsRows, ct))
        {
            if (left is not null)
            {
                martenCount++;
            }

            if (right is not null)
            {
                pbsCount++;
            }

            if (left is not null && left.Removed != left.DocumentRemoved)
            {
                if (left.Removed)
                {
                    unmarkedRemovals++;
                }
                else
                {
                    unexpectedMarks++;
                }
            }

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.YearMonth), right is not null);
            }

            switch (left, right)
            {
                case ({ Removed: false }, null):
                    report.Add(entity, Finding.MissingInPbs, id,
                        $"marten {Category(left!)} {labelA}={Show(left.SegmentA)} {labelB}={Show(left.SegmentB)}", Category(left));
                    break;
                case ({ Removed: true }, not null):
                    report.Add(entity, Finding.StillInPbsAfterRemoval, id, string.Empty, Category(left!));
                    break;
                case (null, not null):
                    report.Add(entity, Finding.OnlyInPbs, id, $"pbs {labelA}={Show(right!.SegmentA)} {labelB}={Show(right.SegmentB)}");
                    break;
                case ({ Removed: false }, not null):
                    compared++;
                    if (left!.SegmentA != right!.SegmentA || left.SegmentB != right.SegmentB)
                    {
                        report.Add(entity, Finding.StaleInPbs, id,
                            $"marten {labelA}/{labelB}={Show(left.SegmentA)}/{Show(left.SegmentB)} "
                            + $"pbs {labelA}/{labelB}={Show(right.SegmentA)}/{Show(right.SegmentB)}", Category(left));
                    }

                    break;
            }
        }

        report.Scanned(entity, martenCount, pbsCount, compared);
        NoteDocumentDrift(report, entity, unmarkedRemovals, unexpectedMarks);
    }

    // The two answers to "should this exist" held against each other. These are not findings about PBS - the
    // events are what PBS is judged by - but they say how far the aggregate's own isRemoved has drifted from them.
    private static void NoteDocumentDrift(Report report, string entity, long unmarkedRemovals, long unexpectedMarks)
    {
        if (unmarkedRemovals > 0)
        {
            report.Note($"{entity}: {unmarkedRemovals:N0} rijen zijn volgens de events verwijderd maar hun "
                        + "aggregate-document staat niet op isRemoved");
        }

        if (unexpectedMarks > 0)
        {
            report.Note($"{entity}: {unexpectedMarks:N0} rijen staan in hun aggregate-document op isRemoved terwijl "
                        + "het laatste bestaansevent op hun stream er een is die ze aanmaakt");
        }
    }

    private static string? DescribeSegmentDifferences(SegmentRow marten, SegmentRow pbs, double lengthTolerance)
    {
        var differences = new List<string>();

        if (marten.StartNode != pbs.StartNode || marten.EndNode != pbs.EndNode)
        {
            differences.Add($"knopen marten={Show(marten.StartNode)}->{Show(marten.EndNode)} pbs={Show(pbs.StartNode)}->{Show(pbs.EndNode)}");
        }

        // Status, but only for a segment that has migrated. The document of one that has not carries no real V2
        // status: MartenMigrationProjection derives a placeholder from the draw method - ingeschetst becomes
        // Gepland, everything else Gerealiseerd - and leaves the actual status in the legacy event it appends.
        // PBS projects that actual status, so holding the two against each other says nothing about PBS. It flagged
        // 97% of every ingeschetst segment before this was skipped.
        //
        // And only when both sides have one: a null on either side means "not comparable", not "different".
        if (marten.HasMigrated && marten.Status is not null && pbs.Status is not null && marten.Status != pbs.Status)
        {
            differences.Add($"status marten={marten.Status} pbs={pbs.Status}");
        }

        if (marten.Points != pbs.Points)
        {
            differences.Add($"punten marten={marten.Points} pbs={pbs.Points}");
        }

        // PBS rounds every coordinate to the centimetre, so allow a little room per vertex on top of the flat
        // tolerance before calling a length difference a stale geometry.
        var allowed = lengthTolerance + 0.005 * Math.Max(marten.Points, pbs.Points);
        if (Math.Abs(marten.Length - pbs.Length) > allowed)
        {
            differences.Add($"lengte marten={Show(marten.Length)} pbs={Show(pbs.Length)}");
        }

        return differences.Count == 0 ? null : string.Join(", ", differences);
    }

    // Walks two id-ordered streams together, so neither side is ever held in memory in full.
    private static async IAsyncEnumerable<(int Id, T? Left, T? Right)> MergeJoin<T>(
        IAsyncEnumerable<(int Id, T Row)> left,
        IAsyncEnumerable<(int Id, T Row)> right,
        [EnumeratorCancellation] CancellationToken ct = default)
        where T : class
    {
        await using var leftEnumerator = left.GetAsyncEnumerator(ct);
        await using var rightEnumerator = right.GetAsyncEnumerator(ct);

        var hasLeft = await leftEnumerator.MoveNextAsync();
        var hasRight = await rightEnumerator.MoveNextAsync();

        while (hasLeft || hasRight)
        {
            if (hasLeft && hasRight && leftEnumerator.Current.Id == rightEnumerator.Current.Id)
            {
                yield return (leftEnumerator.Current.Id, leftEnumerator.Current.Row, rightEnumerator.Current.Row);
                hasLeft = await leftEnumerator.MoveNextAsync();
                hasRight = await rightEnumerator.MoveNextAsync();
            }
            else if (!hasRight || (hasLeft && leftEnumerator.Current.Id < rightEnumerator.Current.Id))
            {
                yield return (leftEnumerator.Current.Id, leftEnumerator.Current.Row, null);
                hasLeft = await leftEnumerator.MoveNextAsync();
            }
            else
            {
                yield return (rightEnumerator.Current.Id, null, rightEnumerator.Current.Row);
                hasRight = await rightEnumerator.MoveNextAsync();
            }
        }
    }

    private static async IAsyncEnumerable<(int Id, T Row)> ReadAsync<T>(
        DbConnection connection, string sql, Func<DbDataReader, T> read, [EnumeratorCancellation] CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 0;   // a full-table scan of the road network outlives any default timeout
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            yield return (reader.GetInt32(0), read(reader));
        }
    }

    private static async Task<object?> ScalarAsync(DbConnection connection, string sql, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteScalarAsync(ct);
    }

    // The document's status column is the bare V2 name Marten serializes. Whether that is that name or its Dutch
    // translation is a serialization detail, so accept either rather than depend on one.
    private static int? ResolveStatus(string value)
    {
        var trimmed = value.Trim();
        foreach (var status in RoadSegmentStatusV2.All)
        {
            if (string.Equals(status.ToString(), trimmed, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status.Translation.Name, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return status.Translation.Identifier;
            }
        }

        return null;
    }

    private static string Show(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "-";

    private static string Show(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
