namespace RoadRegistry.BackOffice.Framework.Containers
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Schema;

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
        }

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

        public async Task<ShapeContext> CreateShapeContextAsync(SqlConnectionStringBuilder builder)
        {
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseSqlServer(builder.ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ShapeContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

        public async Task<ShapeContext> CreateEmptyShapeContextAsync(SqlConnectionStringBuilder builder)
        {
            var context = await CreateShapeContextAsync(builder);

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
