namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Threading.Tasks;
    using Changes;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using NodaTime.Testing;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class ChangeFeedGetNextTests
    {
        private readonly SqlServer _fixture;

        public ChangeFeedGetNextTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task When_downloading_next_changes_without_specifying_a_max_entry_count()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=0")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter is missing.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_with_too_many_max_entry_counts_specified()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=0&maxEntryCount=5&maxEntryCount=10")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter requires exactly 1 value.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_with_a_max_entry_count_that_is_not_an_integer()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=0&maxEntryCount=abc")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("MaxEntryCount query string parameter value must be an integer.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_without_specifying_an_after_entry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?maxEntryCount=0")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("AfterEntry query string parameter is missing.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_with_too_many_after_entries_specified()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=1&afterEntry=2&maxEntryCount=10")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("AfterEntry query string parameter requires exactly 1 value.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_with_an_after_entry_that_is_not_an_integer()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=abc&maxEntryCount=0")
                    }
                }
            }};
            using (var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync()))
            {
                var result = await controller.GetNext(context);

                var badRequest = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("AfterEntry query string parameter value must be an integer.", badRequest.Value);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_of_an_empty_registry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
            {ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?afterEntry=0&maxEntryCount=5")
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
    }
}
