using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class FixSrid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [RoadRegistryWfs].[Wegknoop]
SET [puntGeometrie].STSrid = 31370
WHERE [puntGeometrie].STSrid = 0;

UPDATE [RoadRegistryWfs].[Wegsegment]
SET [middellijnGeometrie].STSrid = 31370
WHERE [middellijnGeometrie].STSrid = 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
