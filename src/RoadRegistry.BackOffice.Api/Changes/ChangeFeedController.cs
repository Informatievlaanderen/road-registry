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

        [HttpGet("")]
        public async Task<IActionResult> Get([FromServices] EditorContext context)
        {
            var entries = new List<ChangeFeedEntry>();
            await context
                .RoadNetworkChanges
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
                        TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay)

                    };
                    switch (change.Type)
                    {
                        case nameof(BeganRoadNetworkImport):
                            item.Content = null;
                            break;
                        case nameof(CompletedRoadNetworkImport):
                            item.Content = null;
                            break;
                        case nameof(RoadNetworkChangesArchiveUploaded):
                            item.Content = JsonConvert.DeserializeObject(change.Content,
                                typeof(RoadNetworkChangesArchiveUploadedEntry));
                            break;
                        case nameof(RoadNetworkChangesArchiveAccepted):
                            item.Content = JsonConvert.DeserializeObject(change.Content,
                                typeof(RoadNetworkChangesArchiveAcceptedEntry));
                            break;
                        case nameof(RoadNetworkChangesArchiveRejected):
                            item.Content = JsonConvert.DeserializeObject(change.Content,
                                typeof(RoadNetworkChangesArchiveRejectedEntry));
                            break;
                        case nameof(RoadNetworkChangesBasedOnArchiveAccepted):
                            item.Content = JsonConvert.DeserializeObject(change.Content,
                                typeof(RoadNetworkChangesBasedOnArchiveAcceptedEntry));
                            break;
                        case nameof(RoadNetworkChangesBasedOnArchiveRejected):
                            item.Content = JsonConvert.DeserializeObject(change.Content,
                                typeof(RoadNetworkChangesBasedOnArchiveRejectedEntry));
                            break;
                    }

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
    }
}
