namespace RoadRegistry.Framework.Containers
{
    using System;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Product.Schema;

    public class SqlServer : ISqlServerDatabase
    {
        private readonly ISqlServerDatabase _inner;

        public SqlServer()
        {
            if (Environment.GetEnvironmentVariable("CI") == null)
            {
                _inner = new SqlServerEmbeddedContainer();
            }
            else
            {
                _inner = new SqlServerComposedContainer();
            }

            MemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public RecyclableMemoryStreamManager MemoryStreamManager { get; }

        public Task InitializeAsync()
        {
            return _inner.InitializeAsync();
        }

        public Task DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
        {
            return _inner.CreateDatabaseAsync();
        }

        public async Task<EditorContext> CreateEditorContextAsync(SqlConnectionStringBuilder builder)
        {
            var options = new DbContextOptionsBuilder<EditorContext>()
                .UseSqlServer(builder.ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new EditorContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

        public async Task<EditorContext> CreateEmptyEditorContextAsync(SqlConnectionStringBuilder builder)
        {
            var context = await CreateEditorContextAsync(builder);

            context.Organizations.RemoveRange(context.Organizations);
            context.RoadNodes.RemoveRange(context.RoadNodes);
            context.RoadSegments.RemoveRange(context.RoadSegments);
            context.RoadSegmentEuropeanRoadAttributes.RemoveRange(context.RoadSegmentEuropeanRoadAttributes);
            context.RoadSegmentNationalRoadAttributes.RemoveRange(context.RoadSegmentNationalRoadAttributes);
            context.RoadSegmentNumberedRoadAttributes.RemoveRange(context.RoadSegmentNumberedRoadAttributes);
            context.RoadSegmentLaneAttributes.RemoveRange(context.RoadSegmentLaneAttributes);
            context.RoadSegmentWidthAttributes.RemoveRange(context.RoadSegmentWidthAttributes);
            context.RoadSegmentSurfaceAttributes.RemoveRange(context.RoadSegmentSurfaceAttributes);
            context.GradeSeparatedJunctions.RemoveRange(context.GradeSeparatedJunctions);
            context.RoadNetworkInfo.RemoveRange(context.RoadNetworkInfo);
            context.ProjectionStates.RemoveRange(context.ProjectionStates);
            await context.SaveChangesAsync();

            return context;
        }

        public async Task<ProductContext> CreateProductContextAsync(SqlConnectionStringBuilder builder)
        {
            var options = new DbContextOptionsBuilder<ProductContext>()
                .UseSqlServer(builder.ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ProductContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

        public async Task<ProductContext> CreateEmptyProductContextAsync(SqlConnectionStringBuilder builder)
        {
            var context = await CreateProductContextAsync(builder);

            context.Organizations.RemoveRange(context.Organizations);
            context.RoadNodes.RemoveRange(context.RoadNodes);
            context.RoadSegments.RemoveRange(context.RoadSegments);
            context.RoadSegmentEuropeanRoadAttributes.RemoveRange(context.RoadSegmentEuropeanRoadAttributes);
            context.RoadSegmentNationalRoadAttributes.RemoveRange(context.RoadSegmentNationalRoadAttributes);
            context.RoadSegmentNumberedRoadAttributes.RemoveRange(context.RoadSegmentNumberedRoadAttributes);
            context.RoadSegmentLaneAttributes.RemoveRange(context.RoadSegmentLaneAttributes);
            context.RoadSegmentWidthAttributes.RemoveRange(context.RoadSegmentWidthAttributes);
            context.RoadSegmentSurfaceAttributes.RemoveRange(context.RoadSegmentSurfaceAttributes);
            context.GradeSeparatedJunctions.RemoveRange(context.GradeSeparatedJunctions);
            context.RoadNetworkInfo.RemoveRange(context.RoadNetworkInfo);
            context.ProjectionStates.RemoveRange(context.ProjectionStates);
            await context.SaveChangesAsync();

            return context;
        }

        public async Task EnsureWmsSchemaAsync(SqlConnectionStringBuilder builder)
        {
            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

                const string createTableScript = @"SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
CREATE TABLE [dbo].[wegsegmentDeNorm](
	[wegsegmentID] [int] NOT NULL,
	[methode] [int] NULL,
	[beheerder] [varchar](18) NULL,
	[begintijd] [datetime] NULL,
	[beginoperator] [varchar](100) NULL,
	[beginorganisatie] [varchar](18) NULL,
	[beginapplicatie] [varchar](100) NULL,
	[geometrie] [geometry] NULL,
	[morfologie] [int] NULL,
	[status] [int] NULL,
	[categorie] [varchar](10) NULL,
	[beginWegknoopID] [int] NULL,
	[eindWegknoopID] [int] NULL,
	[linksStraatnaamID] [int] NULL,
	[rechtsStraatnaamID] [int] NULL,
	[wegsegmentversie] [int] NULL,
	[geometrieversie] [int] NULL,
	[opnamedatum] [datetime] NULL,
	[toegangsbeperking] [int] NULL,
	[transactieID] [int] NULL,
	[sourceID] [int] NULL,
	[bronSourceID] [varchar](18) NULL,
	[linksGemeente] [int] NULL,
	[rechtsGemeente] [int] NULL,
	[lblCategorie] [varchar](64) NULL,
	[lblMethode] [varchar](64) NULL,
	[lblMorfologie] [varchar](64) NULL,
	[lblToegangsbeperking] [varchar](64) NULL,
	[lblStatus] [varchar](64) NULL,
	[lblOrganisatie] [varchar](64) NULL,
	[linksStraatnaam] [char](128) NULL,
	[rechtsStraatnaam] [char](128) NULL,
	[lblBeheerder] [varchar](64) NULL,
	[geometrie2D] [geometry] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
ALTER TABLE [dbo].[wegsegmentDeNorm] ADD  CONSTRAINT [PK_wegsegmentDeNormWMS] PRIMARY KEY CLUSTERED
(
	[wegsegmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];

CREATE NONCLUSTERED INDEX [idxwegsegmentmorfologie] ON [dbo].[wegsegmentDeNorm]
(
	[morfologie] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;
CREATE SPATIAL INDEX [SPATIAL_wegsegmentDeNorm0708] ON [dbo].[wegsegmentDeNorm]
(
	[geometrie]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET NUMERIC_ROUNDABORT OFF;
CREATE SPATIAL INDEX [SPATIAL_wegsegmentDeNorm2D0708] ON [dbo].[wegsegmentDeNorm]
(
	[geometrie2D]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
";
                using (var cmd = new SqlCommand(createTableScript, conn))
                {
                    cmd.CommandText = createTableScript;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
