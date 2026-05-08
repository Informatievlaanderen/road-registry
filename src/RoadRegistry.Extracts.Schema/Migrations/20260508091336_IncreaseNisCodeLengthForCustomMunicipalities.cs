using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseNisCodeLengthForCustomMunicipalities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE [RoadRegistryExtracts].[InwinningRoadSegments]
ALTER COLUMN [NisCode] nvarchar(30) null
GO

DROP INDEX [SPATIAL_Inwinningszones] ON [RoadRegistryExtracts].[Inwinningszones]
DROP INDEX [IX_Inwinningszones_NisCode] ON [RoadRegistryExtracts].[Inwinningszones]
ALTER TABLE [RoadRegistryExtracts].[Inwinningszones] DROP CONSTRAINT [PK_Inwinningszones]

GO

ALTER TABLE [RoadRegistryExtracts].[Inwinningszones]
ALTER COLUMN [NisCode] nvarchar(30) not null
GO

ALTER TABLE [RoadRegistryExtracts].[Inwinningszones] ADD  CONSTRAINT [PK_Inwinningszones] PRIMARY KEY CLUSTERED
(
	[NisCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Inwinningszones_NisCode] ON [RoadRegistryExtracts].[Inwinningszones]
(
	[NisCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


CREATE SPATIAL INDEX [SPATIAL_Inwinningszones] ON [RoadRegistryExtracts].[Inwinningszones]
(
	[Contour]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
