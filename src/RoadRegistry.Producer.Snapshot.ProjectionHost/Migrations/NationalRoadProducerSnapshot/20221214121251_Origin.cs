using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.NationalRoadProducerSnapshot
{
    public partial class Origin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Origin_BeginTime",
                schema: "RoadRegistryNationalRoadProducerSnapshotSchema",
                table: "NationalRoad",
                newName: "Origin_Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Origin_Timestamp",
                schema: "RoadRegistryNationalRoadProducerSnapshotSchema",
                table: "NationalRoad",
                newName: "Origin_BeginTime");
        }
    }
}
