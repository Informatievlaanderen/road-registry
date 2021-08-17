namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Changes;
    using Editor.Schema;
    using Editor.Schema.RoadNetworkChanges;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Text;
    using RoadRegistry.Framework.Containers;
    using Xunit;
    using AcceptedChange = Editor.Schema.RoadNetworkChanges.AcceptedChange;
    using RejectedChange = Editor.Schema.RoadNetworkChanges.RejectedChange;

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
                var result = await controller.GetNext(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
                Assert.Empty(response.Entries);
            }
        }

        [Fact]
        public async Task When_downloading_next_changes_of_filled_registry()
        {
            var controller = new ChangeFeedController(new FakeClock(NodaConstants.UnixEpoch))
                {ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Request =
                        {
                            QueryString = new QueryString("?afterEntry=1&maxEntryCount=2")
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
                        Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
                    }),
                    When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
                });
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Id = 1,
                    Title = "Het opladings archief werd geaccepteerd.",
                    Type = nameof(RoadNetworkChangesArchiveAccepted),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveAcceptedEntry
                    {
                        Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                        Files = new FileProblems[0]
                    }),
                    When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
                });
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Id = 2,
                    Title = "De oplading werd geaccepteerd.",
                    Type = nameof(RoadNetworkChangesAccepted),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesBasedOnArchiveAcceptedEntry
                    {
                        Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                        Changes = new AcceptedChange[0]
                    }),
                    When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
                });
                context.RoadNetworkChanges.Add(new RoadNetworkChange
                {
                    Id = 3,
                    Title = "De oplading werd geweigerd.",
                    Type = nameof(RoadNetworkChangesRejected),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesBasedOnArchiveRejectedEntry
                    {
                        Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                        Changes = new RejectedChange[0]
                    }),
                    When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
                });
                await context.SaveChangesAsync();
            }

            using (var context = await _fixture.CreateEditorContextAsync(database))
            {
                var result = await controller.GetNext(context);

                var jsonResult = Assert.IsType<JsonResult>(result);
                Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
                var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);

                Assert.Equal(2, response.Entries.Length);

                var item1 = response.Entries[0];
                Assert.NotNull(item1);
                Assert.Equal(3, item1.Id);
                Assert.Equal("De oplading werd geweigerd.", item1.Title);
                Assert.Equal(nameof(RoadNetworkChangesRejected), item1.Type);
                Assert.Equal("01", item1.Day);
                // YR: Different versions of libicu use different casing
                Assert.Equal("jan.", item1.Month.ToLowerInvariant());
                Assert.Equal("01:00", item1.TimeOfDay);

                var item2 = response.Entries[1];
                Assert.NotNull(item2);
                Assert.Equal(2, item2.Id);
                Assert.Equal("De oplading werd geaccepteerd.", item2.Title);
                Assert.Equal(nameof(RoadNetworkChangesAccepted), item2.Type);
                Assert.Equal("01", item2.Day);
                // YR: Different versions of libicu use different casing
                Assert.Equal("jan.", item2.Month.ToLowerInvariant());
                Assert.Equal("01:00", item2.TimeOfDay);
            }
        }
    }
}
