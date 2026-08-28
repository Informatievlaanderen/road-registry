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
// index is left alone - disabling it would make the table itself unreadable.
internal static class SqlServerIndexMaintenance
{
    private const int NoTimeout = 0;

    private const string SelectIndexesSql = """
        SELECT s.name AS SchemaName, t.name AS TableName, i.name AS IndexName
        FROM sys.indexes i
        INNER JOIN sys.tables t ON t.object_id = i.object_id
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE s.name IN ({0})
          AND i.name IS NOT NULL
          AND i.type_desc <> 'CLUSTERED'
          AND i.is_primary_key = 0
          AND i.is_unique = 0
          AND i.is_unique_constraint = 0
          AND i.is_disabled = {1}
        """;

    public static async Task<int> DisableAsync(DbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        var indexes = await FindAsync(context, disabled: false, cancellationToken).ConfigureAwait(false);
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
    public static async Task<int> RebuildDisabledAsync(DbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        var indexes = await FindAsync(context, disabled: true, cancellationToken).ConfigureAwait(false);
        if (indexes.Count == 0)
        {
            return 0;
        }

        logger.LogInformation("Rebuilding {Count} disabled index(es) now that the projection is caught up: {Indexes}",
            indexes.Count, string.Join(", ", indexes.Select(x => x.IndexName)));

        foreach (var index in indexes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteAsync(context, $"ALTER INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}] REBUILD", cancellationToken).ConfigureAwait(false);
        }

        return indexes.Count;
    }

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
                results.Add(new IndexReference(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
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

    private sealed record IndexReference(string SchemaName, string TableName, string IndexName);
}
