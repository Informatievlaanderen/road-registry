namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Threading.Tasks;
    using Framework.Containers;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using RoadRegistry.Api.Activities;
    using RoadRegistry.Api.Changes.Responses;
    using Schema;
    using Translation;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class ActivityControllerTests
    {
        private readonly SqlServer _fixture;

        public ActivityControllerTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task When_downloading_activities_of_empty_registry()
        {
            var controller = new ChangeFeedController{ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}};
            using (var context = await _fixture.CreateEmptyShapeContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.Get(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
                Assert.Empty(response.Entries);
            }
        }

        [Fact]
        public async Task When_downloading_activities_of_filled_registry()
        {
            var controller = new ChangeFeedController{ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}};
            var database = await _fixture.CreateDatabaseAsync();
            var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
            using (var context = await _fixture.CreateEmptyShapeContextAsync(database))
            {
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Title = "De oplading werd ontvangen.",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        ArchiveId = archiveId
                    })
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateShapeContextAsync(database))
            {
                var result = await controller.Get(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
                var item = Assert.Single(response.Entries);
                Assert.NotNull(item);
                Assert.Equal(1, item.Id);
                Assert.Equal("De oplading werd ontvangen.", item.Title);
                Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), item.Type);
                var content = Assert.IsType<RoadNetworkChangesArchiveUploadedEntry>(item.Content);
                Assert.Equal(archiveId.ToString(), content.ArchiveId);
            }
        }
    }
}
