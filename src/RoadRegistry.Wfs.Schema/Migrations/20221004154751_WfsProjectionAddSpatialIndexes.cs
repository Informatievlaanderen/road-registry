using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class WfsProjectionAddSpatialIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"CREATE SPATIAL INDEX [SPATIAL_RoadRegistryWfsWegsegment_MiddellijnGeometrie] ON [RoadRegistryWfs].[Wegsegment] ([middellijnGeometrie])\n" +
                @"USING  GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");

            migrationBuilder.Sql(
                $"CREATE SPATIAL INDEX [SPATIAL_RoadRegistryWfsWegknoop_PuntGeometrie] ON [RoadRegistryWfs].[Wegknoop] ([puntGeometrie])\n" +
                @"USING  GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("[SPATIAL_RoadRegistryWfsWegsegment_MiddellijnGeometrie]");
            migrationBuilder.DropIndex("[SPATIAL_RoadRegistryWfsWegknoop_PuntGeometrie]");
        }
    }
}
