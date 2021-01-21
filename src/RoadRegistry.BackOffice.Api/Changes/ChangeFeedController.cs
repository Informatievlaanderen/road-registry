namespace RoadRegistry.BackOffice.Api.Changes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Editor.Schema;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("changefeed")]
    [ApiExplorerSettings(GroupName = "ChangeFeed")]
    public class ChangeFeedController : ControllerBase
    {
        private readonly IClock _clock;
        private readonly DateTimeZone _localTimeZone;
        private readonly LocalTimePattern _localTimeOfDayPattern;
        private readonly LocalDatePattern _localMonthPattern;

        public ChangeFeedController(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _localTimeZone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];
            _localMonthPattern = LocalDatePattern.Create("MMM", new CultureInfo("nl-BE"));
            _localTimeOfDayPattern = LocalTimePattern.CreateWithInvariantCulture("HH':'mm");
        }

        [HttpGet("head")]
        public async Task<IActionResult> GetHead([FromServices] EditorContext context)
        {
            if (!HttpContext.Request.Query.TryGetValue("MaxEntryCount", out var maxEntryCountValue))
            {
                return BadRequest("MaxEntryCount query string parameter is missing.");
            }

            if(maxEntryCountValue.Count != 1)
            {
                return BadRequest("MaxEntryCount query string parameter requires exactly 1 value.");
            }

            if(!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount))
            {
                return BadRequest("MaxEntryCount query string parameter value must be an integer.");
            }

            var entries = new List<ChangeFeedEntry>();
            await context
                .RoadNetworkChanges
                .Select(change => new
                {
                    change.Id,
                    change.Title,
                    change.Type,
                    change.When
                })
                .Take(maxEntryCount)
                .OrderByDescending(_ => _.Id)
                .ForEachAsync(change =>
                {
                    var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                    var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                    var item = new ChangeFeedEntry
                    {
                        Id = change.Id,
                        Title = change.Title,
                        Type = change.Type,
                        Day = localWhen.Day.ToString("00"),
                        Month = _localMonthPattern.Format(localWhen.Date),
                        TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay),
                        ContentLink = $"/entry/{ change.Id.ToString(CultureInfo.InvariantCulture)}/content"
                    };

                    entries.Add(item);
                }, HttpContext.RequestAborted);

            return new JsonResult(new ChangeFeedResponse
            {
                Entries = entries.ToArray()
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpGet("next")]
        public async Task<IActionResult> GetNext([FromServices] EditorContext context)
        {
            if(!HttpContext.Request.Query.TryGetValue("AfterEntry", out var afterEntryValue))
            {
                return BadRequest("AfterEntry query string parameter is missing.");
            }

            if (!HttpContext.Request.Query.TryGetValue("MaxEntryCount", out var maxEntryCountValue))
            {
                return BadRequest("MaxEntryCount query string parameter is missing.");
            }

            if(afterEntryValue.Count != 1)
            {
                return BadRequest("AfterEntry query string parameter requires exactly 1 value.");
            }

            if(maxEntryCountValue.Count != 1)
            {
                return BadRequest("MaxEntryCount query string parameter requires exactly 1 value.");
            }

            if(!long.TryParse(afterEntryValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var afterEntry))
            {
                return BadRequest("AfterEntry query string parameter value must be an integer.");
            }

            if(!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount))
            {
                return BadRequest("MaxEntryCount query string parameter value must be an integer.");
            }

            var entries = new List<ChangeFeedEntry>();
            await context
                .RoadNetworkChanges
                .Where(change => change.Id > afterEntry)
                .Take(maxEntryCount)
                .OrderBy(_ => _.Id)
                .Select(change => new
                {
                    change.Id,
                    change.Title,
                    change.Type,
                    change.When
                })
                .ForEachAsync(change =>
                {
                    var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                    var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                    var item = new ChangeFeedEntry
                    {
                        Id = change.Id,
                        Title = change.Title,
                        Type = change.Type,
                        Day = localWhen.Day.ToString("00"),
                        Month = _localMonthPattern.Format(localWhen.Date),
                        TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay),
                        ContentLink = $"/entry/{ change.Id.ToString(CultureInfo.InvariantCulture)}/content"
                    };
                    entries.Add(item);
                }, HttpContext.RequestAborted);

            return new JsonResult(new ChangeFeedResponse
            {
                Entries = entries.OrderByDescending(entry => entry.Id).ToArray()
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpGet("previous")]
        public async Task<IActionResult> GetPrevious([FromServices] EditorContext context)
        {
            if(!HttpContext.Request.Query.TryGetValue("BeforeEntry", out var beforeEntryValue))
            {
                return BadRequest("BeforeEntry query string parameter is missing.");
            }

            if (!HttpContext.Request.Query.TryGetValue("MaxEntryCount", out var maxEntryCountValue))
            {
                return BadRequest("MaxEntryCount query string parameter is missing.");
            }

            if(beforeEntryValue.Count != 1)
            {
                return BadRequest("BeforeEntry query string parameter requires exactly 1 value.");
            }

            if(maxEntryCountValue.Count != 1)
            {
                return BadRequest("MaxEntryCount query string parameter requires exactly 1 value.");
            }

            if(!long.TryParse(beforeEntryValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var beforeEntry))
            {
                return BadRequest("BeforeEntry query string parameter value must be an integer.");
            }

            if(!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount))
            {
                return BadRequest("MaxEntryCount query string parameter value must be an integer.");
            }

            var entries = new List<ChangeFeedEntry>();
            await context
                .RoadNetworkChanges
                .Where(change => change.Id < beforeEntry)
                .Take(maxEntryCount)
                .OrderByDescending(_ => _.Id)
                .Select(change => new
                {
                    change.Id,
                    change.Title,
                    change.Type,
                    change.When
                })
                .ForEachAsync(change =>
                {
                    var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                    var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                    var item = new ChangeFeedEntry
                    {
                        Id = change.Id,
                        Title = change.Title,
                        Type = change.Type,
                        Day = localWhen.Day.ToString("00"),
                        Month = _localMonthPattern.Format(localWhen.Date),
                        TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay),
                        ContentLink = $"/entry/{ change.Id.ToString(CultureInfo.InvariantCulture)}/content"
                    };
                    entries.Add(item);
                }, HttpContext.RequestAborted);

            return new JsonResult(new ChangeFeedResponse
            {
                Entries = entries.OrderByDescending(entry => entry.Id).ToArray()
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        [HttpGet("entry/{id}/content")]
        public async Task<IActionResult> GetContent([FromServices] EditorContext context, long id)
        {
            var entry = await context
                .RoadNetworkChanges
                .SingleOrDefaultAsync(change => change.Id == id, HttpContext.RequestAborted);
            if (entry == null)
            {
                return NotFound();
            }

            var content = entry.Type switch
            {
                nameof(RoadNetworkChangesArchiveUploaded) => JsonConvert.DeserializeObject(entry.Content,
                    typeof(RoadNetworkChangesArchiveUploadedEntry)),
                nameof(RoadNetworkChangesArchiveAccepted) => JsonConvert.DeserializeObject(entry.Content,
                    typeof(RoadNetworkChangesArchiveAcceptedEntry)),
                nameof(RoadNetworkChangesArchiveRejected) => JsonConvert.DeserializeObject(entry.Content,
                    typeof(RoadNetworkChangesArchiveRejectedEntry)),
                nameof(RoadNetworkChangesAccepted) => JsonConvert.DeserializeObject(entry.Content,
                    typeof(RoadNetworkChangesBasedOnArchiveAcceptedEntry)),
                nameof(RoadNetworkChangesRejected) => JsonConvert.DeserializeObject(entry.Content,
                    typeof(RoadNetworkChangesBasedOnArchiveRejectedEntry)),
                _ => null
            };
            return new JsonResult(new ChangeFeedEntryContent
            {
                Id = entry.Id,
                Type = entry.Type,
                Content = content
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
