namespace RoadRegistry.Api.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Downloads;
    using Framework;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Projections;
    using Xunit;

    public class DownloadControllerTests : IAsyncLifetime
    {
        private readonly DockerSqlServerDatabase _database;

        public DownloadControllerTests()
        {
            _database = new DockerSqlServerDatabase(Guid.NewGuid().ToString("N"));
        }

        [Fact]
        public async Task When_downloading_before_an_import()
        {
            var controller = new DownloadController();
            using (var context = await PrepareContext())
            {
                var result = await controller.Get(context, CancellationToken.None);

                var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
            }
        }

        [Fact]
        public async Task When_downloading_during_an_import()
        {
            var controller = new DownloadController();
            using (var context = await PrepareContext())
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false
                });
                await context.SaveChangesAsync();
            }

            using (var context = await PrepareContext())
            {
                var result = await controller.Get(context, CancellationToken.None);

                var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
                Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
            }
        }

        [Fact]
        public async Task When_downloading_after_an_import()
        {
            var controller = new DownloadController();
            using (var context = await PrepareContext())
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = await PrepareContext())
            {
                var result = await controller.Get(context, CancellationToken.None);

                var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
                Assert.Equal(fileCallbackResult.FileDownloadName, "wegenregister.zip");
            }
        }

        private async Task<ShapeContext> PrepareContext()
        {
            var connectionString = _database.CreateConnectionStringBuilder().ConnectionString;
            var options = new DbContextOptionsBuilder<ShapeContext>()
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ShapeContext(options);
            await context.Database.MigrateAsync();
            return context;
        }

        public Task InitializeAsync()
        {
            return _database.CreateDatabase();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
