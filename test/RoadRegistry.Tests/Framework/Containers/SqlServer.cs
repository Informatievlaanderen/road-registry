namespace RoadRegistry.Framework.Containers
{
    using System;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;

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
    }
}
