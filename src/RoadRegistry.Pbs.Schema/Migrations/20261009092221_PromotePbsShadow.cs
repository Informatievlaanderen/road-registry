using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class PromotePbsShadow : Migration
    {
        // Makes the shadow read model the live one, the same swap PromoteWmsWfsV2Shadow did - and the last one. PBS
        // was the only read model still being rebuilt beside itself, so the shadow scaffolding goes out of the code
        // with this change: the shadow projection, the temp schema, the context factory that scoped a context to it
        // and the bootstrapper that created its tables.
        //
        // The PBS read model was rebuilt beside itself: the same projection, a second time, into the
        // 'RoadRegistryPbsTemp' schema, while the live one in 'RoadRegistryPbs' kept serving. That rebuild is done,
        // so the two change places here.
        //
        // The tables are moved, not copied: ALTER SCHEMA ... TRANSFER renames the schema an existing table belongs
        // to, so the shadow's tables - their rows, their indexes, their statistics - simply become
        // [RoadRegistryPbs].[...] and the ones they replace are dropped. Nothing is read or written row by row,
        // which is what makes this a short, metadata-only transaction. Unlike WmsWfsV2 there are no spatial indexes
        // to build first, so this needs no migration ahead of it.
        //
        // Two things the swap has to take care of beyond the tables:
        //
        //   - Any view standing on the read model being replaced. There are none on [RoadRegistryPbs] today, but a
        //     schema-bound one would make it impossible to drop those tables underneath it, so the same catalog
        //     query the WmsWfsV2 swap used is kept: it asks which views depend on something in the live schema,
        //     drops exactly those, and replays their definitions afterwards from the catalog rather than from a copy
        //     pasted in here. That is the set DROP TABLE blocks on, it keeps being that set when a view is added
        //     later, and it leaves views belonging to anything else alone.
        //
        //   - The projection-state row travels with the tables, under the shadow projection's name. The live
        //     projection looks its position up by its own name, and a missing row means "new read model" - it would
        //     re-initialize and replay from the start. So the row is renamed to the live projection's name, which is
        //     also what carries the shadow's position over to the projection that continues from it.
        //
        // Marten keys its progression by projection name too, in the other database, so it cannot be done from here:
        // promote_pbs_shadow_progression.sql points the live shard at the position the shadow reached. Both run
        // before the daemon starts, under the projector's distributed lock, and a failure of either stops the host -
        // so the daemon never starts on half a swap.
        //
        // Guarded on the shadow having actually produced something: only a 'RoadRegistryPbsTemp' with a
        // projection-state row past zero is promoted. Anywhere else - a fresh database, dev, test, or production,
        // where PBS was never enabled at all - the live read model is left exactly as it is and the empty
        // scaffolding is dropped, because nothing creates or fills it any more.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @shadowPosition bigint = NULL;

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = N'RoadRegistryPbsTemp' AND t.name = N'ProjectionStates')
BEGIN
    SELECT @shadowPosition = [Position]
    FROM [RoadRegistryPbsTemp].[ProjectionStates]
    WHERE [Name] = N'RoadNetworkChangesPbsTempProjection';
END

DECLARE @sql nvarchar(max);

IF @shadowPosition IS NULL OR @shadowPosition <= 0
BEGIN
    -- No shadow worth promoting. Whatever is left of it is scaffolding for a rebuild that is over: nothing creates
    -- or fills the schema any more, so it goes, and the live read model is not touched.
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP TABLE [RoadRegistryPbsTemp].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'RoadRegistryPbsTemp';

    IF @sql <> N'' EXEC sp_executesql @sql;

    IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'RoadRegistryPbsTemp') EXEC(N'DROP SCHEMA [RoadRegistryPbsTemp];');
END
ELSE
BEGIN
    -- The views standing on the read model that is about to be replaced, with their definitions as they stand right
    -- now, to be replayed once the tables underneath them have been. The dependencies are recorded against an
    -- object rather than a name, because a view that blocks a DROP TABLE is schema-bound by definition.
    DECLARE @views TABLE (Id int IDENTITY(1, 1) PRIMARY KEY, [Schema] sysname NOT NULL, [Name] sysname NOT NULL, [Definition] nvarchar(max) NOT NULL);

    INSERT INTO @views ([Schema], [Name], [Definition])
    SELECT s.name, v.name, m.[definition]
    FROM sys.views v
    JOIN sys.schemas s ON s.schema_id = v.schema_id
    JOIN sys.sql_modules m ON m.object_id = v.object_id
    WHERE EXISTS (
        SELECT 1
        FROM sys.sql_expression_dependencies d
        JOIN sys.objects o ON o.object_id = d.referenced_id
        JOIN sys.schemas rs ON rs.schema_id = o.schema_id
        WHERE d.referencing_id = v.object_id
          AND rs.name = N'RoadRegistryPbs');

    SET @sql = N'';
    SELECT @sql = @sql + N'DROP VIEW ' + QUOTENAME([Schema]) + N'.' + QUOTENAME([Name]) + N';' + CHAR(10)
    FROM @views;

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- The read model being replaced. Everything in the schema except the migrations history, which is this
    -- migration's own bookkeeping and belongs to the schema, not to the read model.
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP TABLE [RoadRegistryPbs].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'RoadRegistryPbs'
      AND t.name <> N'__EFMigrationsHistoryPbs';

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- And the shadow takes its place.
    SET @sql = N'';
    SELECT @sql = @sql + N'ALTER SCHEMA [RoadRegistryPbs] TRANSFER [RoadRegistryPbsTemp].' + QUOTENAME(t.name) + N';' + CHAR(10)
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'RoadRegistryPbsTemp';

    IF @sql <> N'' EXEC sp_executesql @sql;

    -- Leaves the schema empty; anything still in it would fail here rather than be left behind unnoticed.
    IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'RoadRegistryPbsTemp') EXEC(N'DROP SCHEMA [RoadRegistryPbsTemp];');

    -- The position the live projection continues from. The row of the read model that was dropped went with it;
    -- this delete is only here so a leftover cannot collide with the rename.
    DELETE FROM [RoadRegistryPbs].[ProjectionStates] WHERE [Name] = N'RoadNetworkChangesPbsProjection';
    UPDATE [RoadRegistryPbs].[ProjectionStates] SET [Name] = N'RoadNetworkChangesPbsProjection' WHERE [Name] = N'RoadNetworkChangesPbsTempProjection';

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
