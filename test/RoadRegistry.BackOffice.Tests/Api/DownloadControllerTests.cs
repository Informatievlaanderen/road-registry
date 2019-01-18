namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Framework.Containers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using RoadRegistry.Api.Downloads;
    using RoadRegistry.Api.Infrastructure;
    using Schema;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class DownloadControllerTests
    {
        private readonly SqlServer _fixture;

        public DownloadControllerTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task When_downloading_before_an_import()
        {
            var controller = new DownloadController();
            using (var context = await _fixture.CreateEmptyShapeContextAsync(await _fixture.CreateDatabaseAsync()))
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
            var database = await _fixture.CreateDatabaseAsync();
            using (var context = await _fixture.CreateEmptyShapeContextAsync(database))
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateShapeContextAsync(database))
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
            var database = await _fixture.CreateDatabaseAsync();
            using (var context = await _fixture.CreateEmptyShapeContextAsync(database))
            {
                context.RoadNetworkInfo.Add(new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = true
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateShapeContextAsync(database))
            {
                var result = await controller.Get(context, CancellationToken.None);

                var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
                Assert.Equal("wegenregister.zip", fileCallbackResult.FileDownloadName);
            }
        }
    }
}
