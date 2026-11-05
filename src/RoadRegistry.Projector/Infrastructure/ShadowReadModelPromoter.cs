namespace RoadRegistry.Projector.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using RoadRegistry.BackOffice;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Projector.Projections;

// A SQL Server read model that can be rebuilt beside itself: the live schema and projection, and the shadow ones that
// a second copy of the same projection fills from the start of the event stream (see the *TempProjection classes).
public sealed record ShadowReadModel(
    string Name,
    string SqlConnectionName,
    string LiveSchema,
    string ShadowSchema,
    string LiveProjectionName,
    string ShadowProjectionName,
    string MigrationsHistoryTable)
{
    public string LiveShardName => $"{LiveProjectionName}:All";
    public string ShadowShardName => $"{ShadowProjectionName}:All";
}

// Runs the promotion as one of the projector's migrations: after the EF and Marten migrations, before the daemon
// starts, under the projector's distributed lock. A failure stops the host, so the daemon never starts on half a swap.
public sealed class ShadowReadModelPromoterFactory : IDbMigratorFactory
{
    private readonly ShadowReadModel _readModel;
    private readonly bool _shadowEnabled;
    private readonly Func<DbContext> _createShadowContext;

    public ShadowReadModelPromoterFactory(ShadowReadModel readModel, bool shadowEnabled, Func<DbContext> createShadowContext)
    {
        _readModel = readModel;
        _shadowEnabled = shadowEnabled;
        _createShadowContext = createShadowContext;
    }

    public IDbMigrator CreateMigrator(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        return new ShadowReadModelPromoter(
            _readModel,
            _shadowEnabled,
            configuration.GetRequiredConnectionString(_readModel.SqlConnectionName),
            configuration.GetRequiredConnectionString(WellKnownConnectionNames.Marten),
            _createShadowContext,
            loggerFactory.CreateLogger<ShadowReadModelPromoter>());
    }
}

// Makes the shadow read model the live one (Projections:{Pbs|WmsWfsV2}:Shadow:Promote).
//
// Safe to leave switched on: it only promotes when there is something worth promoting, and does nothing otherwise.
// Worth promoting means the shadow projection is switched off - a shadow still being written to cannot be swapped -
// its tables are there with a position past zero, and its Marten progression is not behind the live one's. It does
// not have to be at the store position: whatever it is short of, the live projection catches up on after the swap.
//
// The tables are moved, not copied: ALTER SCHEMA ... TRANSFER renames the schema an existing table belongs to, so the
// shadow's tables - their rows, their indexes, their statistics - simply become the live ones and the ones they
// replace are dropped. That keeps the swap itself a short, metadata-only transaction. What a table transfer does not
// carry is put right around it, all of it read from the catalog rather than written down here, so it keeps matching
// whatever the migrations add later:
//
//   - Indexes the migrations put on the live tables in raw SQL - the spatial indexes above all - are not in the EF
//     model the shadow tables were created from. They are built on the shadow tables first, outside the transaction:
//     that is the expensive part, and doing it first means the live schema is never served without them.
//   - Schema-bound views standing on the live tables (the [wms]/[wfs] ones on [road]) block a DROP TABLE. They are
//     taken out of the way and put back by SqlServerViewMaintenance, the same code a rebuild uses for the same
//     problem: from their own definitions, with the indexes on them, under the session options an indexed view
//     needs. The permissions granted on them are granted again. A view that is not schema-bound is in nobody's way:
//     it stays, and resolves to the promoted tables by name.
//   - Permissions granted on the live tables go with the tables that are dropped, and are granted again on the ones
//     that take their place.
//
// Two positions describe how far a read model got. The SQL Server one travels with the tables, under the shadow
// projection's name, and is copied to the live projection's name. Marten's lives in the other database and is keyed by
// projection name - the shard's progression and the per-correlation progressions - so it cannot travel: the live ones
// are replaced by the shadow's afterwards. The shadow's row is left in the live ProjectionStates until that is done,
// as the marker that it still has to be: a host that stops in between finishes the Marten half on its next start.
public sealed class ShadowReadModelPromoter : IDbMigrator
{
    private const int SwapCommandTimeoutInSeconds = 600;

    private readonly ShadowReadModel _readModel;
    private readonly bool _shadowEnabled;
    private readonly string _sqlConnectionString;
    private readonly string _martenConnectionString;
    private readonly Func<DbContext> _createShadowContext;
    private readonly ILogger<ShadowReadModelPromoter> _logger;

    public ShadowReadModelPromoter(
        ShadowReadModel readModel,
        bool shadowEnabled,
        string sqlConnectionString,
        string martenConnectionString,
        Func<DbContext> createShadowContext,
        ILogger<ShadowReadModelPromoter> logger)
    {
        _readModel = readModel;
        _shadowEnabled = shadowEnabled;
        _sqlConnectionString = sqlConnectionString;
        _martenConnectionString = martenConnectionString;
        _createShadowContext = createShadowContext;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var sql = new SqlConnection(_sqlConnectionString);
        await sql.OpenAsync(cancellationToken);

        if (await IsMartenHalfPendingAsync(sql, cancellationToken))
        {
            _logger.LogWarning("The {ReadModel} shadow read model was promoted, but its Marten progressions were not moved yet; finishing that now.", _readModel.Name);
            await PromoteMartenProgressionsAsync(cancellationToken);
            await RemoveMarkerAsync(sql, cancellationToken);
            return;
        }

        var decision = await DecideAsync(sql, cancellationToken);
        if (!decision.Promote)
        {
            _logger.Log(decision.Level, "Not promoting the {ReadModel} shadow read model: {Reason}", _readModel.Name, decision.Reason);
            return;
        }

        _logger.LogWarning("Promoting the {ReadModel} shadow read model in [{ShadowSchema}] to [{LiveSchema}]: {Reason}",
            _readModel.Name, _readModel.ShadowSchema, _readModel.LiveSchema, decision.Reason);

        await RebuildDisabledShadowIndexesAsync(cancellationToken);
        await CopyMissingIndexesToShadowAsync(sql, cancellationToken);
        await SwapAsync(sql, cancellationToken);
        await PromoteMartenProgressionsAsync(cancellationToken);
        await RemoveMarkerAsync(sql, cancellationToken);

        _logger.LogWarning("The {ReadModel} shadow read model is now the live one.", _readModel.Name);
    }

    private async Task<PromotionDecision> DecideAsync(SqlConnection sql, CancellationToken cancellationToken)
    {
        var shadowPosition = await ReadPositionAsync(sql, _readModel.ShadowSchema, _readModel.ShadowProjectionName, cancellationToken);

        if (_shadowEnabled)
        {
            // Not something to shrug off as "nothing to do": both switches on is a configuration that asks for two
            // contradicting things, and promoting a shadow that is still being written to would lose its writes.
            return PromotionDecision.Skip(LogLevel.Error,
                "the shadow projection is still switched on (Shadow:Enabled); switch it off to promote it.");
        }

        if (shadowPosition is null or <= 0)
        {
            return PromotionDecision.Skip(LogLevel.Information,
                $"there is no shadow read model in [{_readModel.ShadowSchema}] that has projected anything.");
        }

        var progressions = await ReadShardProgressionsAsync(cancellationToken);
        progressions.TryGetValue(_readModel.ShadowShardName, out var shadowSequence);
        progressions.TryGetValue(_readModel.LiveShardName, out var liveSequence);

        return Decide(shadowPosition, shadowSequence, liveSequence);
    }

    // Separate from the reads, so the rule itself can be pinned down without a database.
    public static PromotionDecision Decide(long? shadowPosition, long? shadowSequence, long? liveSequence)
    {
        if (shadowPosition is null or <= 0)
        {
            return PromotionDecision.Skip(LogLevel.Information, "the shadow read model has not projected anything.");
        }

        if (shadowSequence is null)
        {
            return PromotionDecision.Skip(LogLevel.Warning, "the shadow projection has no Marten progression.");
        }

        if (liveSequence is not null && shadowSequence < liveSequence)
        {
            return PromotionDecision.Skip(LogLevel.Warning,
                $"the shadow projection is at {shadowSequence}, behind the live one at {liveSequence}; let it catch up first.");
        }

        return PromotionDecision.Go($"the shadow projection is at {shadowSequence}, the live one at {liveSequence?.ToString() ?? "nothing"}.");
    }

    // The shadow projection disables its indexes while it catches up on a large backlog and rebuilds them once it is
    // caught up. Switched off before it got there, it leaves them disabled - and they would be promoted that way.
    private async Task RebuildDisabledShadowIndexesAsync(CancellationToken cancellationToken)
    {
        await using var context = _createShadowContext();
        await SqlServerIndexMaintenance.RebuildDisabledAsync(context, _logger, "before the shadow read model is promoted", cancellationToken);
    }

    private async Task CopyMissingIndexesToShadowAsync(SqlConnection sql, CancellationToken cancellationToken)
    {
        var liveIndexes = await ReadIndexesAsync(sql, _readModel.LiveSchema, cancellationToken);
        var shadowIndexes = await ReadIndexesAsync(sql, _readModel.ShadowSchema, cancellationToken);

        var shadowTables = shadowIndexes.Select(x => x.ObjectName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        shadowTables.UnionWith(await ReadTablesAsync(sql, _readModel.ShadowSchema, cancellationToken));

        foreach (var index in liveIndexes)
        {
            if (!shadowTables.Contains(index.ObjectName))
            {
                continue;
            }

            var onShadowTable = shadowIndexes.Where(x => string.Equals(x.ObjectName, index.ObjectName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (onShadowTable.Any(x => string.Equals(x.IndexName, index.IndexName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (index.IsClustered && onShadowTable.Any(x => x.IsClustered))
            {
                _logger.LogWarning("Not copying the clustered index {Index} on [{Schema}].[{Table}]: the shadow table already has one.",
                    index.IndexName, _readModel.LiveSchema, index.ObjectName);
                continue;
            }

            _logger.LogWarning("Creating index {Index} on [{Schema}].[{Table}], which the live read model has and the shadow one does not.",
                index.IndexName, _readModel.ShadowSchema, index.ObjectName);

            await ExecuteAsync(sql, null, index.Script(_readModel.ShadowSchema), commandTimeoutInSeconds: 0, cancellationToken);
        }
    }

    private async Task SwapAsync(SqlConnection sql, CancellationToken cancellationToken)
    {
        var liveTables = (await ReadTablesAsync(sql, _readModel.LiveSchema, cancellationToken))
            .Where(table => !string.Equals(table, _readModel.MigrationsHistoryTable, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var shadowTables = await ReadTablesAsync(sql, _readModel.ShadowSchema, cancellationToken);

        // Through a context, because that is what SqlServerViewMaintenance works with; it is on the same database, and
        // only its connection is used. Through the execution strategy as well, because the connection is configured to
        // retry and a retrying strategy refuses a transaction it did not open itself: an attempt replays the whole swap,
        // capturing the views again from scratch.
        await using var context = _createShadowContext();
        var strategy = context.Database.CreateExecutionStrategy();
        var (viewCount, viewIndexCount, permissionCount) = await strategy.ExecuteAsync(async token =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(token);
            var connection = (SqlConnection)context.Database.GetDbConnection();
            var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();

            // Read before anything is dropped: these are what the drops take with them.
            var views = await SqlServerViewMaintenance.CaptureAsync(
                context, liveTables.Select(table => $"{Quote(_readModel.LiveSchema)}.{Quote(table)}").ToList(), token);
            var permissions = await ReadPermissionsAsync(connection, sqlTransaction, views, token);

            await SqlServerViewMaintenance.DropAsync(context, views, _logger, token);

            foreach (var table in liveTables)
            {
                await ExecuteAsync(connection, sqlTransaction, $"DROP TABLE {Quote(_readModel.LiveSchema)}.{Quote(table)};", SwapCommandTimeoutInSeconds, token);
            }

            foreach (var table in shadowTables)
            {
                await ExecuteAsync(connection, sqlTransaction, $"ALTER SCHEMA {Quote(_readModel.LiveSchema)} TRANSFER {Quote(_readModel.ShadowSchema)}.{Quote(table)};", SwapCommandTimeoutInSeconds, token);
            }

            // Leaves the schema empty; anything still in it fails the swap here rather than being left behind unnoticed.
            await ExecuteAsync(connection, sqlTransaction, $"DROP SCHEMA {Quote(_readModel.ShadowSchema)};", SwapCommandTimeoutInSeconds, token);

            // The position the live projection continues from: the shadow's, copied to the live projection's name. The
            // shadow's own row stays, as the marker that the Marten half is still to do.
            await ExecuteAsync(connection, sqlTransaction, $@"
DELETE FROM {Quote(_readModel.LiveSchema)}.[ProjectionStates] WHERE [Name] = @live;
SELECT * INTO #promoted FROM {Quote(_readModel.LiveSchema)}.[ProjectionStates] WHERE [Name] = @shadow;
UPDATE #promoted SET [Name] = @live;
INSERT INTO {Quote(_readModel.LiveSchema)}.[ProjectionStates] SELECT * FROM #promoted;
DROP TABLE #promoted;", SwapCommandTimeoutInSeconds, token,
                ("@live", _readModel.LiveProjectionName), ("@shadow", _readModel.ShadowProjectionName));

            await SqlServerViewMaintenance.RecreateAsync(context, views, _logger, token);

            foreach (var permission in permissions)
            {
                await ExecuteAsync(connection, sqlTransaction, permission.Script(), SwapCommandTimeoutInSeconds, token);
            }

            await transaction.CommitAsync(token);

            return (views.Count, views.Sum(view => view.Indexes.Count), permissions.Count);
        }, cancellationToken);

        _logger.LogWarning(
            "Swapped {Tables} table(s) of [{ShadowSchema}] into [{LiveSchema}], replacing {Replaced}; put back {Views} view(s), {ViewIndexes} view index(es) and {Permissions} permission(s).",
            shadowTables.Count, _readModel.ShadowSchema, _readModel.LiveSchema, liveTables.Count, viewCount, viewIndexCount, permissionCount);
    }

    // The live shard is pointed at the position the shadow reached - in both directions: the rows now in the live schema
    // are the ones the shadow wrote, so its position is what describes them. The same goes for the per-correlation
    // progressions, which say up to where a correlation's events were applied: the live projection's describe tables
    // that are gone, so they are replaced by the shadow's under the live projection's name. Everything the shadow owned
    // goes, including the desired state the projections page kept for it.
    //
    // Idempotent: run a second time, there is no shadow row left to copy from, and nothing to delete.
    private async Task PromoteMartenProgressionsAsync(CancellationToken cancellationToken)
    {
        await using var marten = new NpgsqlConnection(_martenConnectionString);
        await marten.OpenAsync(cancellationToken);
        await using var transaction = await marten.BeginTransactionAsync(cancellationToken);

        var schema = WellKnownSchemas.MartenEventStore;
        await using var command = new NpgsqlCommand($@"
INSERT INTO {schema}.mt_event_progression (name, last_seq_id, last_updated)
SELECT @liveShard, last_seq_id, transaction_timestamp()
FROM {schema}.mt_event_progression
WHERE name = @shadowShard
ON CONFLICT ON CONSTRAINT pk_mt_event_progression
DO UPDATE SET last_seq_id = EXCLUDED.last_seq_id, last_updated = transaction_timestamp();

DELETE FROM {schema}.mt_event_progression WHERE name = @shadowShard;
DELETE FROM {schema}.mt_doc_martenprojection_state WHERE id = @shadowShard;

DELETE FROM {schema}.mt_doc_roadnetworkchangesprojection_progression WHERE projection_name = @live;
UPDATE {schema}.mt_doc_roadnetworkchangesprojection_progression
SET id = @live || substr(id, length(@shadow) + 1),
    projection_name = @live,
    data = jsonb_set(jsonb_set(data, '{{id}}', to_jsonb(@live || substr(id, length(@shadow) + 1)), false), '{{projectionName}}', to_jsonb(@live::text), false),
    mt_last_modified = transaction_timestamp()
WHERE projection_name = @shadow;", marten, transaction);
        command.CommandTimeout = SwapCommandTimeoutInSeconds;
        command.Parameters.AddWithValue("liveShard", _readModel.LiveShardName);
        command.Parameters.AddWithValue("shadowShard", _readModel.ShadowShardName);
        command.Parameters.AddWithValue("live", _readModel.LiveProjectionName);
        command.Parameters.AddWithValue("shadow", _readModel.ShadowProjectionName);
        await command.ExecuteNonQueryAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<bool> IsMartenHalfPendingAsync(SqlConnection sql, CancellationToken cancellationToken)
    {
        // A shadow schema that is still there means no swap happened since; the marker can only be left by one.
        if ((await ReadTablesAsync(sql, _readModel.ShadowSchema, cancellationToken)).Count > 0)
        {
            return false;
        }

        return await ReadPositionAsync(sql, _readModel.LiveSchema, _readModel.ShadowProjectionName, cancellationToken) is not null;
    }

    private async Task RemoveMarkerAsync(SqlConnection sql, CancellationToken cancellationToken)
    {
        await ExecuteAsync(sql, null, $"DELETE FROM {Quote(_readModel.LiveSchema)}.[ProjectionStates] WHERE [Name] = @shadow;",
            SwapCommandTimeoutInSeconds, cancellationToken, ("@shadow", _readModel.ShadowProjectionName));
    }

    private async Task<Dictionary<string, long>> ReadShardProgressionsAsync(CancellationToken cancellationToken)
    {
        await using var marten = new NpgsqlConnection(_martenConnectionString);
        await marten.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"SELECT name, last_seq_id FROM {WellKnownSchemas.MartenEventStore}.mt_event_progression WHERE name = @live OR name = @shadow", marten);
        command.Parameters.AddWithValue("live", _readModel.LiveShardName);
        command.Parameters.AddWithValue("shadow", _readModel.ShadowShardName);

        var progressions = new Dictionary<string, long>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            progressions[reader.GetString(0)] = reader.GetInt64(1);
        }

        return progressions;
    }

    private static async Task<long?> ReadPositionAsync(SqlConnection sql, string schema, string projectionName, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(@"
DECLARE @query nvarchar(max) = N'SELECT [Position] FROM ' + QUOTENAME(@schema) + N'.[ProjectionStates] WHERE [Name] = @name';
IF OBJECT_ID(QUOTENAME(@schema) + N'.[ProjectionStates]', N'U') IS NOT NULL
    EXEC sp_executesql @query, N'@name nvarchar(450)', @name = @name;", sql);
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@name", projectionName);

        var position = await command.ExecuteScalarAsync(cancellationToken);
        return position is null or DBNull ? null : Convert.ToInt64(position);
    }

    private static async Task<IReadOnlyList<string>> ReadTablesAsync(SqlConnection sql, string schema, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(@"
SELECT t.name
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schema
ORDER BY t.name;", sql);
        command.Parameters.AddWithValue("@schema", schema);

        var tables = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    // Object-level permissions on the live tables and on the views standing on them, including column-level ones.
    // A REVOKE is not something to put back - revoking removes the row it would have been - so only grants and denies.
    private async Task<IReadOnlyList<PermissionDefinition>> ReadPermissionsAsync(
        SqlConnection sql,
        SqlTransaction transaction,
        IReadOnlyList<SqlServerViewMaintenance.ServingView> views,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(@"
SELECT p.state_desc, p.permission_name, s.name, o.name, c.name, pr.name, o.type
FROM sys.database_permissions p
JOIN sys.objects o ON o.object_id = p.major_id
JOIN sys.schemas s ON s.schema_id = o.schema_id
JOIN sys.database_principals pr ON pr.principal_id = p.grantee_principal_id
LEFT JOIN sys.columns c ON c.object_id = p.major_id AND c.column_id = p.minor_id AND p.minor_id <> 0
WHERE p.class = 1
  AND p.state IN ('G', 'W', 'D')
  AND (o.type = 'U' AND s.name = @schema OR o.type = 'V');", sql, transaction);
        command.Parameters.AddWithValue("@schema", _readModel.LiveSchema);

        var permissions = new List<PermissionDefinition>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var permission = new PermissionDefinition(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetString(5));
            var isView = reader.GetString(6).Trim() == "V";

            if (isView && !views.Any(view => view.Schema == permission.Schema && view.Name == permission.ObjectName))
            {
                continue;
            }

            if (!isView && string.Equals(permission.ObjectName, _readModel.MigrationsHistoryTable, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            permissions.Add(permission);
        }

        return permissions;
    }

    // The rowstore and spatial indexes on the tables of a schema, except the ones that back a constraint:
    // those come with the table definition. Columnstore and XML indexes are not used by these read models.
    private static async Task<IReadOnlyList<IndexDefinition>> ReadIndexesAsync(SqlConnection sql, string schema, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(@"
SELECT
    s.name, o.name, i.name, i.type, i.is_unique, i.filter_definition,
    c.name, ic.is_descending_key, ic.is_included_column, ic.key_ordinal, ic.index_column_id,
    t.tessellation_scheme,
    t.bounding_box_xmin, t.bounding_box_ymin, t.bounding_box_xmax, t.bounding_box_ymax,
    t.level_1_grid_desc, t.level_2_grid_desc, t.level_3_grid_desc, t.level_4_grid_desc,
    t.cells_per_object
FROM sys.indexes i
JOIN sys.objects o ON o.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = o.schema_id
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
LEFT JOIN sys.spatial_index_tessellations t ON t.object_id = i.object_id AND t.index_id = i.index_id
WHERE o.type = 'U'
  AND s.name = @schema
  AND i.type IN (1, 2, 4)
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0
  AND i.is_hypothetical = 0
ORDER BY s.name, o.name, i.name, ic.is_included_column, ic.key_ordinal, ic.index_column_id;", sql);
        command.Parameters.AddWithValue("@schema", schema);

        var indexes = new List<IndexDefinition>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var schemaName = reader.GetString(0);
            var objectName = reader.GetString(1);
            var indexName = reader.GetString(2);

            var index = indexes.LastOrDefault();
            if (index is null || index.SchemaName != schemaName || index.ObjectName != objectName || index.IndexName != indexName)
            {
                index = new IndexDefinition(
                    schemaName,
                    objectName,
                    indexName,
                    reader.GetByte(3),
                    reader.GetBoolean(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(11)
                        ? null
                        : new SpatialTessellation(
                            reader.GetString(11),
                            reader.IsDBNull(12) ? null : reader.GetDouble(12),
                            reader.IsDBNull(13) ? null : reader.GetDouble(13),
                            reader.IsDBNull(14) ? null : reader.GetDouble(14),
                            reader.IsDBNull(15) ? null : reader.GetDouble(15),
                            reader.IsDBNull(16) ? null : reader.GetString(16),
                            reader.IsDBNull(17) ? null : reader.GetString(17),
                            reader.IsDBNull(18) ? null : reader.GetString(18),
                            reader.IsDBNull(19) ? null : reader.GetString(19),
                            reader.IsDBNull(20) ? null : reader.GetInt32(20)));
                indexes.Add(index);
            }

            var column = new IndexColumn(reader.GetString(6), reader.GetBoolean(7));
            if (reader.GetBoolean(8))
            {
                index.IncludedColumns.Add(column.Name);
            }
            else
            {
                index.KeyColumns.Add(column);
            }
        }

        return indexes;
    }

    private static async Task ExecuteAsync(
        SqlConnection sql,
        SqlTransaction? transaction,
        string commandText,
        int commandTimeoutInSeconds,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] parameters)
    {
        await using var command = new SqlCommand(commandText, sql, transaction);
        command.CommandTimeout = commandTimeoutInSeconds;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal static string Quote(string identifier)
    {
        return $"[{identifier.Replace("]", "]]")}]";
    }

    public sealed record PromotionDecision(bool Promote, LogLevel Level, string Reason)
    {
        public static PromotionDecision Go(string reason) => new(true, LogLevel.Warning, reason);
        public static PromotionDecision Skip(LogLevel level, string reason) => new(false, level, reason);
    }

    internal sealed record PermissionDefinition(string State, string Permission, string Schema, string ObjectName, string? Column, string Grantee)
    {
        public string Script()
        {
            var verb = State == "DENY" ? "DENY" : "GRANT";
            var column = Column is null ? string.Empty : $" ({Quote(Column)})";
            var withGrantOption = State == "GRANT_WITH_GRANT_OPTION" ? " WITH GRANT OPTION" : string.Empty;

            return $"{verb} {Permission} ON {Quote(Schema)}.{Quote(ObjectName)}{column} TO {Quote(Grantee)}{withGrantOption};";
        }
    }

    internal sealed record IndexColumn(string Name, bool IsDescending);

    internal sealed record SpatialTessellation(
        string Scheme,
        double? XMin,
        double? YMin,
        double? XMax,
        double? YMax,
        string? Level1,
        string? Level2,
        string? Level3,
        string? Level4,
        int? CellsPerObject);

    internal sealed record IndexDefinition(
        string SchemaName,
        string ObjectName,
        string IndexName,
        byte Type,
        bool IsUnique,
        string? FilterDefinition,
        SpatialTessellation? Tessellation)
    {
        public List<IndexColumn> KeyColumns { get; } = [];
        public List<string> IncludedColumns { get; } = [];

        public bool IsClustered => Type == 1;

        // The index as it stands on the object it was read from, created on the object of the same name in the given
        // schema.
        public string Script(string targetSchema)
        {
            var target = $"{Quote(targetSchema)}.{Quote(ObjectName)}";

            if (Type == 4)
            {
                return ScriptSpatial(target);
            }

            var builder = new StringBuilder("CREATE ");
            if (IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            builder.Append(IsClustered ? "CLUSTERED" : "NONCLUSTERED");
            builder.Append($" INDEX {Quote(IndexName)} ON {target} (");
            builder.Append(string.Join(", ", KeyColumns.Select(column => $"{Quote(column.Name)}{(column.IsDescending ? " DESC" : " ASC")}")));
            builder.Append(')');

            if (IncludedColumns.Count > 0)
            {
                builder.Append($" INCLUDE ({string.Join(", ", IncludedColumns.Select(Quote))})");
            }

            if (FilterDefinition is not null)
            {
                builder.Append($" WHERE {FilterDefinition}");
            }

            builder.Append(';');
            return builder.ToString();
        }

        private string ScriptSpatial(string target)
        {
            var tessellation = Tessellation ?? throw new InvalidOperationException($"Spatial index {IndexName} on {target} has no tessellation.");
            var options = new List<string>();

            if (tessellation.XMin is not null)
            {
                options.Add(FormattableString.Invariant($"BOUNDING_BOX = ({tessellation.XMin}, {tessellation.YMin}, {tessellation.XMax}, {tessellation.YMax})"));
            }

            // The auto grids have a fixed number of levels and do not take a GRIDS option.
            if (tessellation.Scheme is "GEOMETRY_GRID" or "GEOGRAPHY_GRID" && tessellation.Level1 is not null)
            {
                options.Add($"GRIDS = (LEVEL_1 = {tessellation.Level1}, LEVEL_2 = {tessellation.Level2}, LEVEL_3 = {tessellation.Level3}, LEVEL_4 = {tessellation.Level4})");
            }

            if (tessellation.CellsPerObject is not null)
            {
                options.Add($"CELLS_PER_OBJECT = {tessellation.CellsPerObject}");
            }

            var with = options.Count > 0 ? $" WITH ({string.Join(", ", options)})" : string.Empty;
            return $"CREATE SPATIAL INDEX {Quote(IndexName)} ON {target} ({Quote(KeyColumns.Single().Name)}) USING {tessellation.Scheme}{with};";
        }
    }
}
