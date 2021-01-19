namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Threading.Tasks;
    using Changes;
    using Editor.Schema;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Text;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class ChangeFeedGetEntryContentTests
    {
        private readonly SqlServer _fixture;

        public ChangeFeedGetEntryContentTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task When_downloading_entry_content_of_a_non_existing_entry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetContent(context, 0);

                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task When_downloading_entry_content_of_an_existing_entry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?beforeEntry=1&maxEntryCount=5")
                    }
                }
            }};
            var database = await _fixture.CreateDatabaseAsync();
            var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
            using (var context = await _fixture.CreateEmptyEditorContextAsync(database))
            {
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Id = 0,
                    Title = "Het opladings archief werd ontvangen.",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
                    }),
                    When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateEditorContextAsync(database))
            {
                var result = await controller.GetContent(context, 0);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedEntryContent>(jsonResult.Value);
                Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), response.Type);
                var content = Assert.IsType<RoadNetworkChangesArchiveUploadedEntry>(response.Content);
                Assert.Equal(archiveId.ToString(), content.Archive.Id);
                Assert.True(content.Archive.Available);
                Assert.Equal("file.zip", content.Archive.Filename);
            }
        }
    }
}
