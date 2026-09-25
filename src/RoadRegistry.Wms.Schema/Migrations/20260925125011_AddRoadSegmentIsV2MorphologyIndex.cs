using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadSegmentIsV2MorphologyIndex : Migration
    {
        // The index was already created by hand on the databases, under the name SQL Server itself suggested, so it is
        // created here only where it is still missing.
        //
        // The create sits in EXEC on purpose: ONLINE = ON is rejected while a batch is compiled, not while it runs, on
        // editions that do not offer online index operations. Written out in the batch it would fail there even when the
        // guard skips it; inside EXEC it is only compiled when the index is actually missing.
        private const string IndexName = "nci_msft_1_wegsegmentDenorm_0DB1125C0F3780A74F7979055E2293D6";
        private const string IndexExists = $@"
SELECT 1
FROM sys.indexes
WHERE name = N'{IndexName}'
  AND object_id = OBJECT_ID(N'[RoadRegistryWmsData].[wegsegmentDenorm]')";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF NOT EXISTS ({IndexExists})
    EXEC(N'CREATE NONCLUSTERED INDEX [{IndexName}]
ON [RoadRegistryWmsData].[wegsegmentDenorm] ([IsV2], [morfologie])
INCLUDE ([beheerder], [geometrie2D], [toegangsbeperking])
WITH (ONLINE = ON)')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF EXISTS ({IndexExists})
    DROP INDEX [{IndexName}] ON [RoadRegistryWmsData].[wegsegmentDenorm]");
        }
    }
}
