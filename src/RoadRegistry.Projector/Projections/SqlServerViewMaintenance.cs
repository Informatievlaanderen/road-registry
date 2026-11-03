namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

// Takes the views that serve a read model out of the way of a rebuild, and puts them back.
//
// A view created WITH SCHEMABINDING blocks TRUNCATE TABLE on everything it is bound to - that is the whole point of
// the binding - so a rebuild of WmsWfsV2 fails on its first table with "Cannot TRUNCATE TABLE 'road.
// AfgeleideWegsegmenten' because it is being referenced by object 'Wegsegmenten'". Four of those views are indexed
// views as well, which is a second materialized copy of the data, maintained synchronously on every insert: leaving
// them in place would make the replay that follows the truncate pay for them row by row.
//
// WHERE THE DEFINITIONS COME FROM is the only interesting design decision here. They are read out of
// sys.sql_modules at the moment of the rebuild, not out of this repository. A copy in C# - shared with the migrations
// or beside them - is a second source that has to be kept in step with whatever the last migration created, and
// nothing would notice when it was not: the rebuild would quietly put back a stale view. Reading the live definition
// makes that impossible rather than unlikely. The same goes for WHICH views: they are found through
// sys.sql_expression_dependencies, so a view a future migration adds is picked up without anyone remembering to
// register it here.
//
// The catch of a live read is that it is also the only copy: once dropped, a view that is not recreated is gone. So
// the caller runs all of it - capture, drop, truncate, recreate - inside one transaction. TRUNCATE and view DDL are
// both transactional in SQL Server, so a process that dies halfway leaves the read model exactly as it was, views
// and all, instead of without the views that serve it.
//
// Only schema-bound views are touched, which is exactly the set that blocks a TRUNCATE (and, since an indexed view
// has to be schema-bound, every indexed one). A plain view over a truncated table is in nobody's way and returns the
// same nothing whether it is dropped or not, so it is left alone - which also means the PBS rebuild, whose two views
// are not bound, keeps running the path it has always run. Should one of them ever gain SCHEMABINDING or an index, it
// is covered from that moment without anyone having to remember this file.
internal static class SqlServerViewMaintenance
{
    private const int NoTimeout = 0;

    // Creating an index on a view is refused unless the session options are exactly these - ARITHABORT in particular
    // is OFF by default on a SqlClient connection, which fails with "CREATE INDEX failed because the following SET
    // options have incorrect settings". Set inside the caller's transaction, so it is certain to be the same
    // connection that runs the DDL rather than whatever the pool hands out next.
    private const string IndexedViewSessionOptions = """
        SET ANSI_NULLS ON;
        SET ANSI_PADDING ON;
        SET ANSI_WARNINGS ON;
        SET ARITHABORT ON;
        SET CONCAT_NULL_YIELDS_NULL ON;
        SET QUOTED_IDENTIFIER ON;
        SET NUMERIC_ROUNDABORT OFF;
        """;

    // Views that depend on the given tables, directly or through another view, with their own indexes.
    private const string SelectDependentViewsSql = """
        WITH truncated AS (
            -- Cast, because STRING_SPLIT hands back the width of what it was given and OBJECT_ID refuses an
            -- nvarchar(max) argument outright.
            SELECT OBJECT_ID(CAST(value AS nvarchar(776))) AS object_id
            FROM STRING_SPLIT(@tables, '|')
        ),
        dependents AS (
            SELECT d.referencing_id AS object_id
            FROM sys.sql_expression_dependencies d
            WHERE d.referenced_id IN (SELECT object_id FROM truncated WHERE object_id IS NOT NULL)
            UNION ALL
            SELECT d.referencing_id
            FROM sys.sql_expression_dependencies d
            INNER JOIN dependents p ON d.referenced_id = p.object_id
        ),
        -- Made distinct before the definition is joined on, not after: two dependency paths to the same view would
        -- otherwise be two rows, and DISTINCT over an nvarchar(max) body is not what to lean on to collapse them.
        found AS (
            SELECT DISTINCT object_id FROM dependents
        )
        SELECT v.object_id, s.name AS SchemaName, v.name AS ViewName, m.definition AS Definition
        FROM found dep
        INNER JOIN sys.views v ON v.object_id = dep.object_id
        INNER JOIN sys.schemas s ON s.schema_id = v.schema_id
        INNER JOIN sys.sql_modules m ON m.object_id = v.object_id
        WHERE m.is_schema_bound = 1
        ORDER BY s.name, v.name
        OPTION (MAXRECURSION 32)
        """;

    private const string SelectViewIndexesSql = """
        SELECT i.object_id, i.index_id, i.name AS IndexName, i.is_unique, i.type_desc,
               c.name AS ColumnName, ic.is_descending_key, ic.is_included_column
        FROM sys.indexes i
        INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.name IS NOT NULL
          AND i.object_id IN (SELECT CAST(value AS int) FROM STRING_SPLIT(@views, ','))
        ORDER BY i.object_id, i.index_id, ic.is_included_column, ic.key_ordinal, ic.index_column_id
        """;

    internal sealed record ViewIndexColumn(string Name, bool IsDescending);

    internal sealed record ViewIndex(string Name, bool IsUnique, bool IsClustered)
    {
        public List<ViewIndexColumn> KeyColumns { get; } = [];
        public List<string> IncludedColumns { get; } = [];
    }

    internal sealed record ServingView(string Schema, string Name, string Definition)
    {
        public List<ViewIndex> Indexes { get; } = [];

        public string QualifiedName => $"[{Schema}].[{Name}]";
    }

    public static async Task<IReadOnlyList<ServingView>> CaptureAsync(
        DbContext context, IReadOnlyCollection<string> qualifiedTableNames, CancellationToken cancellationToken)
    {
        if (qualifiedTableNames.Count == 0)
        {
            return [];
        }

        var views = new Dictionary<int, ServingView>();
        await using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            Prepare(context, command, SelectDependentViewsSql, new SqlParameter("@tables", string.Join('|', qualifiedTableNames)));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var objectId = reader.GetInt32(0);
                var schema = reader.GetString(1);
                var name = reader.GetString(2);

                // NULL for an encrypted module. Recreating it would be impossible, so this stops before anything is
                // dropped rather than after.
                if (await reader.IsDBNullAsync(3, cancellationToken).ConfigureAwait(false))
                {
                    throw new InvalidOperationException(
                        $"The definition of view [{schema}].[{name}] cannot be read from sys.sql_modules, so the rebuild "
                        + "cannot put it back after truncating the tables it depends on. Refusing to drop it.");
                }

                views[objectId] = new ServingView(schema, name, reader.GetString(3));
            }
        }

        if (views.Count == 0)
        {
            return [];
        }

        await using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            Prepare(context, command, SelectViewIndexesSql, new SqlParameter("@views", string.Join(',', views.Keys)));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            ViewIndex? current = null;
            var currentKey = (ObjectId: 0, IndexId: 0);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var key = (ObjectId: reader.GetInt32(0), IndexId: reader.GetInt32(1));
                if (current is null || key != currentKey)
                {
                    current = new ViewIndex(reader.GetString(2), reader.GetBoolean(3), reader.GetString(4) == "CLUSTERED");
                    currentKey = key;
                    views[key.ObjectId].Indexes.Add(current);
                }

                if (reader.GetBoolean(7))
                {
                    current.IncludedColumns.Add(reader.GetString(5));
                }
                else
                {
                    current.KeyColumns.Add(new ViewIndexColumn(reader.GetString(5), reader.GetBoolean(6)));
                }
            }
        }

        return views.Values.ToList();
    }

    // Dropped in whatever order succeeds: a view bound to another view cannot go first, and the dependency direction
    // is not worth a second catalog query when trying is this cheap.
    public static Task DropAsync(DbContext context, IReadOnlyList<ServingView> views, ILogger logger, CancellationToken cancellationToken)
    {
        if (views.Count == 0)
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("Dropping {Count} view(s) that depend on the tables being truncated, to be recreated "
                              + "from their current definition once it is done: {Views}",
            views.Count, string.Join(", ", views.Select(x => x.QualifiedName)));

        return ExecuteUntilSettledAsync(context,
            views.Select(view => (view.QualifiedName, Sql: $"DROP VIEW {view.QualifiedName};")).ToList(),
            cancellationToken);
    }

    // And created in whatever order succeeds, for the same reason the other way round.
    public static async Task RecreateAsync(DbContext context, IReadOnlyList<ServingView> views, ILogger logger, CancellationToken cancellationToken)
    {
        if (views.Count == 0)
        {
            return;
        }

        await ExecuteAsync(context, IndexedViewSessionOptions, cancellationToken).ConfigureAwait(false);

        await ExecuteUntilSettledAsync(context,
            views.Select(view => (view.QualifiedName, view.Definition)).ToList(),
            cancellationToken).ConfigureAwait(false);

        var indexes = views.SelectMany(view => view.Indexes.Select(index => (view, index))).ToList();
        foreach (var (view, index) in indexes)
        {
            await ExecuteAsync(context, CreateIndexStatement(view, index), cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("Recreated {Views} view(s) and {Indexes} view index(es).", views.Count, indexes.Count);
    }

    // Pure, and the one piece here with a shape worth asserting: everything else is a catalog read or a string the
    // server itself handed us back.
    internal static string CreateIndexStatement(ServingView view, ViewIndex index)
    {
        var unique = index.IsUnique ? "UNIQUE " : string.Empty;
        var clustered = index.IsClustered ? "CLUSTERED" : "NONCLUSTERED";
        var keys = string.Join(", ", index.KeyColumns.Select(column => $"[{column.Name}]{(column.IsDescending ? " DESC" : " ASC")}"));
        var included = index.IncludedColumns.Count > 0
            ? $" INCLUDE ({string.Join(", ", index.IncludedColumns.Select(column => $"[{column}]"))})"
            : string.Empty;

        return $"CREATE {unique}{clustered} INDEX [{index.Name}] ON {view.QualifiedName} ({keys}){included};";
    }

    // Runs the statements in passes, keeping whatever failed for the next one, until a pass changes nothing. That
    // settles any ordering the dependencies impose without this having to know what it is; a statement that is simply
    // wrong still surfaces, as the error of the pass that made no progress.
    private static async Task ExecuteUntilSettledAsync(
        DbContext context, List<(string Label, string Sql)> statements, CancellationToken cancellationToken)
    {
        var remaining = statements;
        while (remaining.Count > 0)
        {
            var failed = new List<(string Label, string Sql)>();
            Exception? lastFailure = null;

            foreach (var statement in remaining)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await ExecuteAsync(context, statement.Sql, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    failed.Add(statement);
                    lastFailure = exception;
                }
            }

            if (failed.Count == remaining.Count)
            {
                throw new InvalidOperationException(
                    $"Could not run the view DDL for {string.Join(", ", failed.Select(x => x.Label))}. The transaction "
                    + "this runs in is rolled back, so the read model keeps the views it had.", lastFailure);
            }

            remaining = failed;
        }
    }

    // Awaited inside the try, not returned from it: a `return` would reset the timeout the moment the task was
    // handed back rather than when the command had run.
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

    // The caller holds a transaction, so the command has to join it rather than open a connection of its own.
    private static void Prepare(DbContext context, System.Data.Common.DbCommand command, string sql, SqlParameter parameter)
    {
        command.CommandText = sql;
        command.CommandTimeout = NoTimeout;
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        command.Parameters.Add(parameter);
    }
}
