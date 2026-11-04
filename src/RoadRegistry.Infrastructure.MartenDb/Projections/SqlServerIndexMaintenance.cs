namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Disables the non-clustered indexes on a projection's own tables for the duration of a catch-up and rebuilds them
// afterwards. A rebuild writes far more rows than it reads, and every non-clustered index is another write per row -
// the flattened road segment table alone carries two dozen of them.
//
// DISABLE rather than DROP on purpose: a disabled index keeps its definition, so it is rebuilt from the model that
// created it and there is no DDL here to drift out of step with the migrations.
//
// The scope is deliberately narrow: only tables in the schemas this DbContext maps, only non-clustered indexes, and
// never a unique index or one backing a constraint (disabling those would take the constraint with it). The clustered
// index is left alone - disabling it would make the table itself unreadable. A caller may narrow it further by naming
// the leading key columns whose indexes are the projection's own read path and have to survive the catch-up.
public static class SqlServerIndexMaintenance
{
    private const int NoTimeout = 0;

    private const string SelectIndexesSql = """
        SELECT s.name AS SchemaName, t.name AS TableName, i.name AS IndexName, c.name AS LeadingColumnName
        FROM sys.indexes i
        INNER JOIN sys.tables t ON t.object_id = i.object_id
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        -- Left, not inner: an index without a key column at ordinal 1 (a columnstore, say) has no leading column to
        -- spare it, and must still come back so it is disabled rather than silently kept.
        LEFT JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
        LEFT JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE s.name IN ({0})
          AND i.name IS NOT NULL
          AND i.type_desc <> 'CLUSTERED'
          AND i.is_primary_key = 0
          AND i.is_unique = 0
          AND i.is_unique_constraint = 0
          AND i.is_disabled = {1}
        """;

    public static async Task<int> DisableAsync(DbContext context, ILogger logger, IReadOnlyCollection<string> keepLeadingOn, CancellationToken cancellationToken)
    {
        var found = await FindAsync(context, disabled: false, cancellationToken).ConfigureAwait(false);

        var spared = keepLeadingOn.Count == 0
            ? []
            : found.Where(x => LeadsOn(x, keepLeadingOn)).ToList();
        if (spared.Count > 0)
        {
            logger.LogInformation("Keeping {Count} index(es) in place, leading on a column this projection reads by: {Indexes}",
                spared.Count, string.Join(", ", spared.Select(x => x.IndexName)));
        }

        // A column that spares nothing is the failure this option can have: a rename or a typo leaves the projection's
        // own read path to be disabled with everything else, and the only symptom is a rebuild that crawls. It has to
        // be said out loud, because the alternative is reading it out of the absence of a line.
        if (keepLeadingOn.Count > 0)
        {
            var unmatched = keepLeadingOn.Where(column => !found.Any(x => LeadsOn(x, [column]))).ToList();
            if (unmatched.Count > 0)
            {
                // An index an earlier run disabled is not in `found`, so ask the catalog for those too before calling
                // a column unknown - otherwise a resumed rebuild warns about every index it already turned off.
                var alreadyDisabled = await FindAsync(context, disabled: true, cancellationToken).ConfigureAwait(false);
                unmatched = unmatched.Where(column => !alreadyDisabled.Any(x => LeadsOn(x, [column]))).ToList();
            }

            if (unmatched.Count > 0)
            {
                logger.LogWarning(
                    "No index on this read model leads on {Columns}, so nothing was spared for them. Check the "
                    + "KeepIndexesLeadingOn setting against the column names in the model: a lookup whose index is "
                    + "disabled becomes a table scan for the whole catch-up.",
                    string.Join(", ", unmatched));
            }
        }

        var indexes = found.Except(spared).ToList();
        if (indexes.Count == 0)
        {
            return 0;
        }

        logger.LogInformation("Disabling {Count} non-clustered index(es) for the duration of the catch-up: {Indexes}",
            indexes.Count, string.Join(", ", indexes.Select(x => x.IndexName)));

        foreach (var index in indexes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteAsync(context, $"ALTER INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}] DISABLE", cancellationToken).ConfigureAwait(false);
        }

        return indexes.Count;
    }

    // Rebuilding only the disabled indexes keeps this cheap to call unconditionally: on a host that never disabled
    // anything it is a single catalog query. That is what repairs a host which crashed mid-catch-up and left its
    // indexes disabled - the next start finds them and rebuilds.
    //
    // The reason is logged rather than inferred because the two callers are opposites: one has finished with the
    // indexes off, the other is refusing to turn them off at all and is undoing an earlier run's decision.
    public static async Task<int> RebuildDisabledAsync(DbContext context, ILogger logger, string reason, CancellationToken cancellationToken)
    {
        var indexes = await FindAsync(context, disabled: true, cancellationToken).ConfigureAwait(false);
        if (indexes.Count == 0)
        {
            return 0;
        }

        logger.LogInformation("Rebuilding {Count} disabled index(es) {Reason}: {Indexes}",
            indexes.Count, reason, string.Join(", ", indexes.Select(x => x.IndexName)));

        foreach (var index in indexes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteAsync(context, $"ALTER INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}] REBUILD", cancellationToken).ConfigureAwait(false);
        }

        return indexes.Count;
    }

    // The names of the indexes currently disabled, for a caller that wants to report the state rather than change it.
    public static async Task<IReadOnlyList<string>> FindDisabledAsync(DbContext context, CancellationToken cancellationToken)
    {
        var indexes = await FindAsync(context, disabled: true, cancellationToken).ConfigureAwait(false);
        return indexes.Select(x => x.IndexName).ToList();
    }

    private static bool LeadsOn(IndexReference index, IReadOnlyCollection<string> columns)
        => index.LeadingColumnName is not null && columns.Contains(index.LeadingColumnName, StringComparer.OrdinalIgnoreCase);

    private static async Task<IReadOnlyList<IndexReference>> FindAsync(DbContext context, bool disabled, CancellationToken cancellationToken)
    {
        var schemas = context.Model
            .GetEntityTypes()
            .Select(entityType => entityType.GetSchema())
            .Where(schema => !string.IsNullOrEmpty(schema))
            .Select(schema => schema!)
            .Distinct()
            .ToList();
        if (schemas.Count == 0)
        {
            return [];
        }

        var parameterNames = schemas.Select((_, i) => $"@schema{i}").ToList();
        var sql = string.Format(SelectIndexesSql, string.Join(", ", parameterNames), disabled ? "1" : "0");

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = NoTimeout;
            for (var i = 0; i < schemas.Count; i++)
            {
                var parameter = new SqlParameter(parameterNames[i], schemas[i]);
                command.Parameters.Add(parameter);
            }

            var results = new List<IndexReference>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(new IndexReference(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3)));
            }

            return results;
        }
        finally
        {
            if (shouldClose)
            {
                await context.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }
    }

    private static async Task ExecuteAsync(DbContext context, string sql, CancellationToken cancellationToken)
    {
        var previousTimeout = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(NoTimeout);
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            context.Database.SetCommandTimeout(previousTimeout);
        }
    }

    private sealed record IndexReference(string SchemaName, string TableName, string IndexName, string? LeadingColumnName);
}
