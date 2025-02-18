using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Product.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationV2",
                schema: "RoadRegistryProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OvoCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    KboNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsMaintainer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationV2", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationV2_Code",
                schema: "RoadRegistryProduct",
                table: "OrganizationV2",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationV2_Id",
                schema: "RoadRegistryProduct",
                table: "OrganizationV2",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationV2_IsMaintainer",
                schema: "RoadRegistryProduct",
                table: "OrganizationV2",
                column: "IsMaintainer");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationV2_KboNumber",
                schema: "RoadRegistryProduct",
                table: "OrganizationV2",
                column: "KboNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationV2_OvoCode",
                schema: "RoadRegistryProduct",
                table: "OrganizationV2",
                column: "OvoCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationV2",
                schema: "RoadRegistryProduct");
        }
    }
}
