namespace RoadRegistry.Api.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Downloads;
    using Framework;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Projections;
    using Xunit;

    [Collection(nameof(SqlServerDatabaseCollection))]
    public class DownloadControllerTests
    {
        private readonly SqlServerDatabaseFixture _fixture;

        public DownloadControllerTests(SqlServerDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task When_downloading_before_an_import()
        {
            var controller = new DownloadController();
            using (var context = await _fixture.CreateEmptyShapeContextAsync())
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
            using (var context = await _fixture.CreateEmptyShapeContextAsync())
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateShapeContextAsync())
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
            using (var context = await _fixture.CreateEmptyShapeContextAsync())
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateShapeContextAsync())
            {
                var result = await controller.Get(context, CancellationToken.None);

                var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
                Assert.Equal("wegenregister.zip", fileCallbackResult.FileDownloadName);
            }
        }
    }
}
