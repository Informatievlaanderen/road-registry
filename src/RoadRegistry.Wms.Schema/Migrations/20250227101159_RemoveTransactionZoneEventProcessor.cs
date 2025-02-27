using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    using BackOffice;

    /// <inheritdoc />
    public partial class RemoveTransactionZoneEventProcessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM [{WellKnownSchemas.WmsMetaSchema}].[ProjectionStates] WHERE [Name] = 'roadregistry-wms-transactionzone-projectionhost'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
