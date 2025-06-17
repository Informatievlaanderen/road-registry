using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_beginWegknoopID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "beginWegknoopID")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_beheerder",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "beheerder")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_categorie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "categorie")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_eindWegknoopID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "eindWegknoopID")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_linksStraatnaam",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "linksStraatnaam")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_methode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "methode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_rechtsStraatnaam",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "rechtsStraatnaam")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_status",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "status")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_toegangsbeperking",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "toegangsbeperking")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_beginWegknoopID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_beheerder",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_categorie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_eindWegknoopID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_linksStraatnaam",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_methode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_rechtsStraatnaam",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_status",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_toegangsbeperking",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
