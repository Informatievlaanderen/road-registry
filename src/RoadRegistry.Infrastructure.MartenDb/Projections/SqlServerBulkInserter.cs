namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

// Writes a batch's pending inserts with SqlBulkCopy instead of EF's row-by-row INSERT batching. A rebuild is almost
// entirely inserts and the fan-out tables (the flattened segments, the per-attribute tables, the road number tables)
// produce many rows per road segment, which is where EF's per-row command building and change tracking cost the most.
//
// Only entity types whose primary key is store-generated (an identity column) are eligible. That restriction is what
// makes it safe to bulk-copy before EF applies the batch's deletes: with a surrogate identity key an inserted row can
// never collide with a row the same batch is about to delete. Tables with application-assigned keys (the road segment
// and road node rows themselves, the label caches) stay on the EF path, where the delete/insert ordering is handled
// for us. Those are one row per segment anyway - the volume is in the fan-out tables.
internal static class SqlServerBulkInserter
{
    // One entity type's worth of pending inserts, already flattened to provider values in column order.
    internal sealed record BulkInsertBatch(
        IEntityType EntityType,
        string DestinationTableName,
        IReadOnlyList<string> Columns,
        IReadOnlyList<object?[]> Rows,
        IReadOnlyList<EntityEntry> Entries);

    // Collects the Added entries that are worth bulk-copying, without touching the change tracker: the caller only
    // detaches them once the copy has actually succeeded, so a failure can still fall back to the EF path.
    public static IReadOnlyList<BulkInsertBatch> Collect(DbContext context, int threshold)
    {
        var batches = new List<BulkInsertBatch>();

        var candidates = context.ChangeTracker
            .Entries()
            .Where(entry => entry.State == EntityState.Added)
            .GroupBy(entry => entry.Metadata);

        foreach (var group in candidates)
        {
            var entityType = group.Key;
            if (!IsEligible(entityType))
            {
                continue;
            }

            var entries = group.ToList();
            if (entries.Count < threshold)
            {
                continue;
            }

            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            if (tableName is null)
            {
                continue;
            }

            var storeObject = StoreObjectIdentifier.Table(tableName, schema);
            var properties = entityType
                .GetProperties()
                .Where(property => !IsStoreGenerated(property))
                .Where(property => property.GetComputedColumnSql() is null)
                .Where(property => property.GetColumnName(storeObject) is not null)
                .ToArray();
            if (properties.Length == 0)
            {
                continue;
            }

            var columns = properties.Select(property => property.GetColumnName(storeObject)!).ToArray();
            var geometryWriter = new SqlServerBytesWriter { IsGeography = false };
            var rows = new List<object?[]>(entries.Count);

            foreach (var entry in entries)
            {
                var values = new object?[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = ToProviderValue(properties[i], entry.CurrentValues[properties[i]], geometryWriter);
                }

                rows.Add(values);
            }

            batches.Add(new BulkInsertBatch(
                entityType,
                schema is null ? $"[{tableName}]" : $"[{schema}].[{tableName}]",
                columns,
                rows,
                entries));
        }

        return batches;
    }

    // Runs on the context's own connection and ambient transaction, so the copied rows commit together with whatever
    // SaveChangesAsync writes afterwards - including the projection state position.
    public static async Task WriteAsync(DbContext context, IReadOnlyList<BulkInsertBatch> batches, CancellationToken cancellationToken)
    {
        if (batches.Count == 0)
        {
            return;
        }

        var connection = context.Database.GetDbConnection() as SqlConnection
                         ?? throw new InvalidOperationException("Bulk insert requires a SQL Server connection.");
        var transaction = context.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction
                          ?? throw new InvalidOperationException("Bulk insert requires an ambient SQL Server transaction.");

        foreach (var batch in batches)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = batch.DestinationTableName,
                // The whole point is one round trip's worth of streaming, so do not chop it up further, and do not let
                // a large rebuild batch trip the default 30s timeout.
                BatchSize = 0,
                BulkCopyTimeout = 0
            };

            for (var i = 0; i < batch.Columns.Count; i++)
            {
                bulkCopy.ColumnMappings.Add(i, batch.Columns[i]);
            }

            using var reader = new RowDataReader(batch.Columns, batch.Rows);
            await bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
        }
    }

    public static void Detach(DbContext context, IReadOnlyList<BulkInsertBatch> batches)
    {
        foreach (var entry in batches.SelectMany(batch => batch.Entries))
        {
            entry.State = EntityState.Detached;
        }
    }

    public static int RowCount(IReadOnlyList<BulkInsertBatch> batches)
    {
        return batches.Sum(batch => batch.Rows.Count);
    }

    // A surrogate, store-generated single-column key is what guarantees an inserted row cannot collide with a row the
    // same batch deletes. Anything else - composite keys, application-assigned keys, owned types - stays on EF.
    private static bool IsEligible(IEntityType entityType)
    {
        if (entityType.IsOwned() || entityType.GetNavigations().Any())
        {
            return false;
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey is null || primaryKey.Properties.Count != 1)
        {
            return false;
        }

        return IsStoreGenerated(primaryKey.Properties[0]);
    }

    private static bool IsStoreGenerated(IProperty property)
    {
        return property.ValueGenerated.HasFlag(ValueGenerated.OnAdd);
    }

    private static object? ToProviderValue(IProperty property, object? value, SqlServerBytesWriter geometryWriter)
    {
        if (value is null)
        {
            return null;
        }

        var converter = property.GetValueConverter() ?? property.FindTypeMapping()?.Converter;
        if (converter is not null)
        {
            value = converter.ConvertToProvider(value);
        }

        // The SQL Server spatial types travel as their serialized binary form; that is exactly what the EF provider
        // writes for a geometry column, and what SqlBulkCopy needs since it cannot convert an NTS instance itself.
        if (value is Geometry geometry)
        {
            value = geometryWriter.Write(geometry);
        }

        return value;
    }

    // SqlBulkCopy only ever pulls values forward, so this reader implements the reading half and refuses the rest
    // rather than pretending to support a general IDataReader.
    private sealed class RowDataReader : IDataReader
    {
        private readonly IReadOnlyList<string> _columns;
        private readonly IReadOnlyList<object?[]> _rows;
        private int _index = -1;

        public RowDataReader(IReadOnlyList<string> columns, IReadOnlyList<object?[]> rows)
        {
            _columns = columns;
            _rows = rows;
        }

        public int FieldCount => _columns.Count;
        public bool IsClosed => _index >= _rows.Count;
        public int Depth => 0;
        public int RecordsAffected => -1;

        public bool Read()
        {
            _index++;
            return _index < _rows.Count;
        }

        public object GetValue(int i)
        {
            return _rows[_index][i] ?? DBNull.Value;
        }

        public bool IsDBNull(int i)
        {
            return _rows[_index][i] is null;
        }

        public string GetName(int i)
        {
            return _columns[i];
        }

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                if (string.Equals(_columns[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException(name);
        }

        public Type GetFieldType(int i)
        {
            foreach (var row in _rows)
            {
                if (row[i] is { } value)
                {
                    return value.GetType();
                }
            }

            return typeof(object);
        }

        public int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, FieldCount);
            for (var i = 0; i < count; i++)
            {
                values[i] = GetValue(i);
            }

            return count;
        }

        public void Close()
        {
            _index = _rows.Count;
        }

        public void Dispose()
        {
            Close();
        }

        public bool NextResult()
        {
            return false;
        }

        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));

        public string GetDataTypeName(int i) => GetFieldType(i).Name;
        public DataTable GetSchemaTable() => throw new NotSupportedException();
        public bool GetBoolean(int i) => (bool)GetValue(i);
        public byte GetByte(int i) => (byte)GetValue(i);
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public char GetChar(int i) => (char)GetValue(i);
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public IDataReader GetData(int i) => throw new NotSupportedException();
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public string GetString(int i) => (string)GetValue(i);
    }
}
