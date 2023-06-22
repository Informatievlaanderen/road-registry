using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class FixSrid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [RoadRegistryWms].[wegsegmentDenorm]
SET [geometrie2D].STSrid = 31370
WHERE [geometrie2D].STSrid = 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
