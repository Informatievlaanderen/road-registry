using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WegknopenDataAndProductViews : Migration
    {
        // The road nodes move behind a view per download product, the same shape the WFS read model took in
        // MoveTablesToDataSchemaBehindViewsExcludingV2: the projection keeps writing one row per road node, now into
        // [WegknopenData], and team Datapublicatie reads [Wegknopen] for the basisdownload and [AfgeleideWegknopen]
        // for the afgeleide download.
        //
        // Both views select the same rows and the same columns today - the split is what this migration is for, the
        // difference comes next (GAWR-7836): the basisdownload is to keep only the v1 schijnknopen, while the
        // afgeleide download also carries the v2 schijnknopen that splitting a dynamically segmented wegsegment
        // adds. Until then the only thing that changes for a consumer is the name it reads, and only for the
        // afgeleide download, which is new.
        //
        // The views name their columns: a column added to [WegknopenData] later has to be added to its views as
        // well. They are not schema-bound, so that column can be added - and the table can be dropped and rebuilt,
        // which a shadow rebuild does - without dropping the views first.
        //
        // The table stays in [RoadRegistryPbs] and only changes name, so it keeps its owner and its grants, and the
        // views read it through that same ownership chain.
        private const string BasisProductView = @"
CREATE VIEW [RoadRegistryPbs].[Wegknopen] AS
SELECT
     [WK_OIDN]
    ,[TYPE]
    ,[LBLTYPE]
    ,[GRENSKNOOP]
    ,[GEOMETRIE]
    ,[CREATIE]
    ,[VERSIE]
FROM [RoadRegistryPbs].[WegknopenData]";

        private const string DerivedProductView = @"
CREATE VIEW [RoadRegistryPbs].[AfgeleideWegknopen] AS
SELECT
     [WK_OIDN]
    ,[TYPE]
    ,[LBLTYPE]
    ,[GRENSKNOOP]
    ,[GEOMETRIE]
    ,[CREATIE]
    ,[VERSIE]
FROM [RoadRegistryPbs].[WegknopenData]";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Wegknopen",
                schema: "RoadRegistryPbs",
                newName: "WegknopenData",
                newSchema: "RoadRegistryPbs");

            // The primary key follows the table in name only. EF scaffolds this as a drop and an add, which on a
            // clustered key rebuilds every row of the table twice; renaming the constraint reaches the same end
            // state as metadata.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_Wegknopen' AND schema_id = SCHEMA_ID(N'RoadRegistryPbs'))
    EXEC sp_rename N'[RoadRegistryPbs].[PK_Wegknopen]', N'PK_WegknopenData', N'OBJECT';");

            migrationBuilder.Sql(BasisProductView);
            migrationBuilder.Sql(DerivedProductView);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW [RoadRegistryPbs].[AfgeleideWegknopen];
DROP VIEW [RoadRegistryPbs].[Wegknopen];");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'PK_WegknopenData' AND schema_id = SCHEMA_ID(N'RoadRegistryPbs'))
    EXEC sp_rename N'[RoadRegistryPbs].[PK_WegknopenData]', N'PK_Wegknopen', N'OBJECT';");

            migrationBuilder.RenameTable(
                name: "WegknopenData",
                schema: "RoadRegistryPbs",
                newName: "Wegknopen",
                newSchema: "RoadRegistryPbs");
        }
    }
}
