using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class PromoteWmsWfsV2Shadow : Migration
    {
        // Makes the shadow read model the live one.
        //
        // The WmsWfsV2 read model was rebuilt beside itself: the same projection, a second time, into the 'roadTemp'
        // schema, while the live one in 'road' kept serving (see the shadow projections). That rebuild is done, so the
        // two change places here - and with this the shadow projection, its schema and the scaffolding that created it
        // are removed from the code.
        //
        // The tables are moved, not copied: ALTER SCHEMA ... TRANSFER renames the schema an existing table belongs to,
        // so the shadow's tables - their rows, their indexes, their statistics - simply become [road].[...] and the
        // ones they replace are dropped. Nothing is read or written row by row, which is what makes the swap itself a
        // short, metadata-only transaction. The one thing the shadow tables are missing, the spatial indexes, is put
        // there by the migration before this one.
        //
        // Three things the swap has to take care of beyond the tables:
        //
        //   - The [wms] and [wfs] views are WITH SCHEMABINDING on [road], which is what keeps them honest and what
        //     makes it impossible to drop those tables underneath them. They are dropped first and put back after,
        //     from the definitions in the catalog rather than from a copy pasted in here: those views have already
        //     been rewritten by five migrations, and a copy would silently revert whatever is added after this one.
        //
        //   - The projection-state row travels with the tables, under the shadow projection's name. The live
        //     projection looks its position up by its own name, and a missing row means "new read model" - it would
        //     re-initialize and replay from the start. So the row is renamed to the live projection's name, which is
        //     also what carries the shadow's position over to the projection that continues from it.
        //
        //   - Marten keys its progression by projection name too, in the other database, so it cannot be done from
        //     here: a migration of its own (promote_wmswfsv2_shadow_progression.sql) points the live shard at the
        //     position the shadow reached. Both run before the daemon starts, under the projector's distributed lock,
        //     and a failure of either stops the host - so the daemon never starts on half a swap.
        //
        // Guarded on the shadow having actually produced something: only a 'roadTemp' with a projection-state row past
        // zero is promoted. Anywhere else - a fresh database, dev, test, or an environment where the shadow schema was
        // created but never filled - the live read model is left exactly as it is and the empty scaffolding is dropped,
        // because nothing creates or fills it any more.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @shadowPosition bigint = NULL;

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = N'roadTemp' AND t.name = N'ProjectionStates')
BEGIN
    SELECT @shadowPosition = [Position]
    FROM [roadTemp].[ProjectionStates]
    WHERE [Name] = N'RoadNetworkChangesWmsWfsV2TempProjection';
END

DECLARE @sql nvarchar(max);

IF @shadowPosition IS NULL OR @shadowPosition <= 0
BEGIN
    -- No shadow worth promoting. Whatever is left of it is scaffolding for a rebuild that is over: nothing creates
    -- or fills the schema any more, so it goes, and the live read model is not touched.
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP TABLE [roadTemp].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'roadTemp';

    IF @sql <> N'' EXEC sp_executesql @sql;

    IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'roadTemp') EXEC(N'DROP SCHEMA [roadTemp];');
END
ELSE
BEGIN
    -- The view definitions as they stand right now, to be replayed once the tables underneath them have been replaced.
    DECLARE @views TABLE (Id int IDENTITY(1, 1) PRIMARY KEY, [Definition] nvarchar(max) NOT NULL);

    INSERT INTO @views ([Definition])
    SELECT m.[definition]
    FROM sys.sql_modules m
    JOIN sys.views v ON v.object_id = m.object_id
    JOIN sys.schemas s ON s.schema_id = v.schema_id
    WHERE s.name IN (N'wms', N'wfs');

    -- Schema-bound, so they have to go before anything underneath them can be dropped.
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP VIEW ' + QUOTENAME(s.name) + N'.' + QUOTENAME(v.name) + N';' + CHAR(10)
    FROM sys.views v
    JOIN sys.schemas s ON s.schema_id = v.schema_id
    WHERE s.name IN (N'wms', N'wfs');

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- The read model being replaced. Everything in the schema except the migrations history, which is this
    -- migration's own bookkeeping and belongs to the schema, not to the read model.
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP TABLE [road].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'road'
      AND t.name <> N'__EFMigrationsHistoryWmsWfsV2';

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- And the shadow takes its place.
    SET @sql = N'';
    SELECT @sql = @sql + N'ALTER SCHEMA [road] TRANSFER [roadTemp].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'roadTemp';

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- Leaves the schema empty; anything still in it would fail here rather than be left behind unnoticed.
    IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'roadTemp') EXEC(N'DROP SCHEMA [roadTemp];');

    -- The position the live projection continues from. The row of the read model that was dropped went with it; this
    -- delete is only here so a leftover cannot collide with the rename.
    DELETE FROM [road].[ProjectionStates] WHERE [Name] = N'RoadNetworkChangesWmsWfsV2Projection';
    UPDATE [road].[ProjectionStates] SET [Name] = N'RoadNetworkChangesWmsWfsV2Projection' WHERE [Name] = N'RoadNetworkChangesWmsWfsV2TempProjection';

    -- The views again, now over the promoted tables.
    DECLARE @viewId int = 1;
    DECLARE @lastViewId int = (SELECT ISNULL(MAX(Id), 0) FROM @views);
    DECLARE @viewDefinition nvarchar(max);

    WHILE @viewId <= @lastViewId
    BEGIN
        SELECT @viewDefinition = [Definition] FROM @views WHERE Id = @viewId;
        EXEC sp_executesql @viewDefinition;
        SET @viewId = @viewId + 1;
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to go back to: the read model that was here is gone, and the one that replaced it is the only
            // copy. Rebuilding it is what the projections rebuild endpoint is for.
        }
    }
}
