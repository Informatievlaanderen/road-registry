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
using RoadRegistry.ValueObjects;

// The Marten side is read from the network topology projection rather than from the snapshot documents: it is an
// INLINE projection, so it is written in the same transaction that appends the events and is therefore never behind
// the event store - exactly the reference a completeness check needs. It also stores what we compare as plain
// columns (id, is_removed, status, the node ids, the geometry), so nothing here depends on a json property name.
//
// What each side is asked for is deliberately shallow: does the row exist, is it gone when it should be gone, and do
// the few fields that change on almost every edit still line up. This finds events PBS never applied; it is not a
// value-by-value audit of the attribute tables.
internal static class Comparers
{
    private const string PbsSchema = WellKnownSchemas.PbsSchema;
    private static readonly string EventStoreSchema = WellKnownSchemas.MartenEventStore;

    internal sealed record SegmentRow(bool Removed, bool IsV2, int? StartNode, int? EndNode, int? Status, string? StatusText, int Points, double Length, DateTimeOffset Timestamp);

    internal sealed record NodeRow(bool Removed, bool IsV2, double X, double Y, DateTimeOffset Timestamp);

    internal sealed record JunctionRow(bool Removed, bool IsV2, int? SegmentA, int? SegmentB, DateTimeOffset Timestamp);

    // Why a row is in the state it is in, counted over every finding. This is what turns "33.234 ontbreken" into a
    // diagnosis: a V1-only row (the topology projection writes no status for those) is a different story from one
    // that a merge or split retired.
    private static string Category(SegmentRow row) => $"{Version(row.IsV2)}, status={row.StatusText ?? "-"}";

    private static string Category(NodeRow row) => Version(row.IsV2);

    private static string Category(JunctionRow row) => Version(row.IsV2);

    private static string Version(bool isV2) => isV2 ? "v2" : "v1";

    // The topology row's event timestamp, by month: a gap that clusters in one window is an incident, a gap that
    // is spread evenly is something systematic.
    private static string Month(DateTimeOffset timestamp)
        => timestamp == default ? "-" : timestamp.ToString("yyyy-MM", CultureInfo.InvariantCulture);

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

    public static async Task CompareSegmentsAsync(DbConnection marten, DbConnection pbs, CompareOptions options, Report report, CancellationToken ct)
    {
        const string entity = "wegsegment";
        var unparseableStatus = 0;
        long martenCount = 0, pbsCount = 0, compared = 0;

        var martenRows = ReadAsync(marten, $"""
            select id, is_removed, status, start_node_id, end_node_id,
                   coalesce(st_npoints(geometry), 0), coalesce(st_length(geometry), 0), is_v2, timestamp
            from {RoadNetworkTopologyProjection.RoadSegmentsTableName}
            order by id
            """,
            reader =>
            {
                int? status = null;
                var statusText = reader.IsDBNull(2) ? null : reader.GetString(2);
                if (statusText is not null)
                {
                    status = ResolveStatus(statusText);
                    if (status is null)
                    {
                        unparseableStatus++;
                    }
                }

                return new SegmentRow(
                    reader.GetBoolean(1),
                    reader.GetBoolean(7),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    status,
                    statusText,
                    reader.GetInt32(5),
                    reader.GetDouble(6),
                    reader.GetFieldValue<DateTimeOffset>(8));
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
                reader.IsDBNull(1) ? null : reader.GetInt32(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                null,
                reader.GetInt32(4),
                reader.GetDouble(5),
                default), ct);

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

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.Timestamp), right is not null);
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
        if (unparseableStatus > 0)
        {
            report.Note($"{unparseableStatus:N0} wegsegmenten hadden een status die deze tool niet kon lezen; status is voor die rijen niet vergeleken.");
        }
    }

    public static async Task CompareNodesAsync(DbConnection marten, DbConnection pbs, Report report, CancellationToken ct)
    {
        const string entity = "wegknoop";
        long martenCount = 0, pbsCount = 0, compared = 0;

        var martenRows = ReadAsync(marten, $"""
            select id, is_removed, coalesce(st_x(geometry), 0), coalesce(st_y(geometry), 0), is_v2, timestamp
            from {RoadNetworkTopologyProjection.RoadNodesTableName}
            order by id
            """,
            reader => new NodeRow(reader.GetBoolean(1), reader.GetBoolean(4), reader.GetDouble(2), reader.GetDouble(3),
                reader.GetFieldValue<DateTimeOffset>(5)), ct);

        var pbsRows = ReadAsync(pbs, $"""
            select [WK_OIDN], coalesce([GEOMETRIE].STX, 0), coalesce([GEOMETRIE].STY, 0)
            from [{PbsSchema}].[WegknopenData]
            order by [WK_OIDN]
            """,
            reader => new NodeRow(false, false, reader.GetDouble(1), reader.GetDouble(2), default), ct);

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

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.Timestamp), right is not null);
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
    }

    public static Task CompareGradeSeparatedJunctionsAsync(DbConnection marten, DbConnection pbs, Report report, CancellationToken ct)
        => CompareJunctionsAsync(
            marten, pbs, report, ct,
            entity: "ongelijkgrondse kruising",
            // BO = bovenliggend = upper, ON = onderliggend = lower.
            martenSql: $"""
                select id, is_removed, upper_road_segment_id, lower_road_segment_id, is_v2, timestamp
                from {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName}
                order by id
                """,
            pbsSql: $"""
                select [OK_OIDN], [BO_WS_OIDN], [ON_WS_OIDN]
                from [{PbsSchema}].[OngelijkgrondseKruisingen]
                order by [OK_OIDN]
                """,
            labelA: "boven", labelB: "onder");

    public static Task CompareGradeJunctionsAsync(DbConnection marten, DbConnection pbs, Report report, CancellationToken ct)
        => CompareJunctionsAsync(
            marten, pbs, report, ct,
            entity: "gelijkgrondse kruising",
            martenSql: $"""
                select id, is_removed, road_segment_id_1, road_segment_id_2, is_v2, timestamp
                from {RoadNetworkTopologyProjection.GradeJunctionsTableName}
                order by id
                """,
            pbsSql: $"""
                select [GK_OIDN], [WS1_OIDN], [WS2_OIDN]
                from [{PbsSchema}].[GelijkgrondseKruisingen]
                order by [GK_OIDN]
                """,
            labelA: "ws1", labelB: "ws2");

    private static async Task CompareJunctionsAsync(
        DbConnection marten, DbConnection pbs, Report report, CancellationToken ct,
        string entity, string martenSql, string pbsSql, string labelA, string labelB)
    {
        long martenCount = 0, pbsCount = 0, compared = 0;

        var martenRows = ReadAsync(marten, martenSql,
            reader => new JunctionRow(
                reader.GetBoolean(1),
                reader.GetBoolean(4),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.GetFieldValue<DateTimeOffset>(5)), ct);

        var pbsRows = ReadAsync(pbs, pbsSql,
            reader => new JunctionRow(
                false,
                false,
                reader.IsDBNull(1) ? null : reader.GetInt32(1),
                reader.IsDBNull(2) ? null : reader.GetInt32(2),
                default), ct);

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

            // Only what Marten says should exist: of every category, how much of it reached PBS.
            if (left is { Removed: false })
            {
                report.Population(entity, Category(left), Month(left.Timestamp), right is not null);
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
    }

    private static string? DescribeSegmentDifferences(SegmentRow marten, SegmentRow pbs, double lengthTolerance)
    {
        var differences = new List<string>();

        if (marten.StartNode != pbs.StartNode || marten.EndNode != pbs.EndNode)
        {
            differences.Add($"knopen marten={Show(marten.StartNode)}->{Show(marten.EndNode)} pbs={Show(pbs.StartNode)}->{Show(pbs.EndNode)}");
        }

        // Only when both sides have a status: a null on either side means "not comparable", not "different".
        if (marten.Status is not null && pbs.Status is not null && marten.Status != pbs.Status)
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

    // The topology projection stores the status as text. Whether that is the V2 name or its Dutch translation is an
    // implementation detail of that projection, so accept either rather than depend on one.
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
