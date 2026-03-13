using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.RoadNetwork.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeJunctionSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "GradeJunctionId",
                schema: "RoadNetwork");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "GradeJunctionId",
                schema: "RoadNetwork");
        }
    }
}
