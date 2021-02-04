namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Globalization;
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
    public class ChangeFeedGetHeadTests
    {
        private readonly SqlServer _fixture;

        public ChangeFeedGetHeadTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task When_downloading_head_changes_without_specifying_a_max_entry_count()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetHead(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter is missing.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_head_changes_with_too_many_max_entry_counts_specified()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?maxEntryCount=5&maxEntryCount=10")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetHead(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter requires exactly 1 value.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_head_changes_with_a_max_entry_count_that_is_not_an_integer()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?maxEntryCount=abc")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetHead(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter value must be an integer.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_head_changes_of_an_empty_registry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?maxEntryCount=5")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetHead(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
                Assert.Empty(response.Entries);
            }
        }

        [Fact]
        public async Task When_downloading_head_changes_of_filled_registry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
                {ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Request =
                        {
                            QueryString = new QueryString("?maxEntryCount=5")
                        }
                    }
                }};
            var database = await _fixture.CreateDatabaseAsync();
            var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
            using (var context = await _fixture.CreateEmptyEditorContextAsync(database))
            {
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Title = "De oplading werd ontvangen.",
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
                var result = await controller.GetHead(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
                var item = Assert.Single(response.Entries);
                Assert.NotNull(item);
                Assert.Equal(0, item.Id);
                Assert.Equal("De oplading werd ontvangen.", item.Title);
                Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), item.Type);
                Assert.Equal("01", item.Day);
                // YR: Different versions of libicu use different casing
                Assert.Equal("jan.", item.Month.ToLowerInvariant());
                Assert.Equal("01:00", item.TimeOfDay);
            }
        }
    }
}
