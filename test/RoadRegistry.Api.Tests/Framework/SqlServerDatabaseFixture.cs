namespace RoadRegistry.Api.Tests.Framework
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Projections;
    using Xunit;

    public class SqlServerDatabaseFixture : IAsyncLifetime
    {
        public SqlServerDatabaseFixture()
        {
            Database = new SqlServerDatabase(Guid.NewGuid().ToString("N"));
        }

        public SqlServerDatabase Database { get; }

        public async Task<ShapeContext> CreateShapeContextAsync()
        {
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseSqlServer(Database.CreateConnectionStringBuilder().ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ShapeContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

        public async Task<ShapeContext> CreateEmptyShapeContextAsync()
        {
            var context = await CreateShapeContextAsync();

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

        public Task InitializeAsync()
        {
            return Database.CreateDatabase();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
