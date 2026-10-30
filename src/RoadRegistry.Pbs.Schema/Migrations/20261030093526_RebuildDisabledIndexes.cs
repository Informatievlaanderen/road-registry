using RoadRegistry.BackOffice;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RebuildDisabledIndexes : Migration
    {
        // Puts back the non-clustered indexes that a catch-up switched off and never switched back on.
        //
        // The projection disables them for the duration of a rebuild, on the reasoning that a rebuild writes far more
        // rows than it reads and every non-clustered index is a second write per row. For this read model that trade
        // is a loss in both directions: every index here is on WS_OIDN, and WS_OIDN is the only way the projection
        // reaches an existing segment's rows - the clustered key of each of these tables is its own identity. So the
        // writes the disabling saves are paid for many times over by the ten-odd lookups each event then has to run
        // as a full table scan. The option is off for this projection from now on; this repairs the databases that
        // already ran with it on, where the indexes would otherwise stay disabled until the projection reaches the
        // tail - which, at scan speed, it does not.
        //
        // DISABLE empties a non-clustered index: the definition stays, the pages are gone, and the optimizer ignores
        // it. There is no ENABLE - REBUILD is what builds the data back - so this is not index maintenance in the
        // usual sense and has nothing to do with fragmentation.
        //
        // Driven off sys.indexes rather than a list of names, which makes it idempotent and resumable: it only
        // touches indexes that are disabled right now, so a database where they are all in place is a catalog query
        // and nothing else, and a run cut short by a timeout picks up the rest when it is tried again. That matters
        // here, because rebuilding an index over a million-row table on a storage-bound server is not quick.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = WellKnownSchemas.PbsSchema;

            // Outside the migration transaction: each ALTER INDEX is its own unit of work, and holding a transaction
            // open across all of them would turn a resumable sequence into one that has to succeed whole.
            migrationBuilder.Sql($@"
DECLARE @indexes TABLE (Id int IDENTITY(1,1) PRIMARY KEY, [Statement] nvarchar(max) NOT NULL);

INSERT INTO @indexes ([Statement])
SELECT N'ALTER INDEX ' + QUOTENAME(i.[name]) + N' ON ' + QUOTENAME(s.[name]) + N'.' + QUOTENAME(t.[name]) + N' REBUILD;'
FROM sys.indexes i
INNER JOIN sys.tables t ON t.object_id = i.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.[name] = N'{schema}'
  AND i.[name] IS NOT NULL
  AND i.is_disabled = 1;

DECLARE @id int = 1;
DECLARE @max int = (SELECT ISNULL(MAX(Id), 0) FROM @indexes);
DECLARE @sql nvarchar(max);
WHILE @id <= @max
BEGIN
    SELECT @sql = [Statement] FROM @indexes WHERE Id = @id;
    EXEC sp_executesql @sql;
    SET @id = @id + 1;
END;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: an index that is in place is the state the model describes, and disabling it again
            // would be a deliberate step backwards rather than a reversal.
        }
    }
}
