using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "toegangsbeperking",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                type: "nvarchar(64)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_beginknoopObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "beginknoopObjectId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_eindknoopObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "eindknoopObjectId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_methodeWegsegmentgeometrie",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "methodeWegsegmentgeometrie")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_morfologischeWegklasse",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "morfologischeWegklasse")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_toegangsbeperking",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "toegangsbeperking")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_wegbeheerder",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "wegbeheerder")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_wegcategorie",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "wegcategorie")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_wegsegmentstatus",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "wegsegmentstatus")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_beginknoopObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_eindknoopObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_methodeWegsegmentgeometrie",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_morfologischeWegklasse",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_toegangsbeperking",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_wegbeheerder",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_wegcategorie",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_wegsegmentstatus",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.AlterColumn<string>(
                name: "toegangsbeperking",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
