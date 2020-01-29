namespace RoadRegistry.Api.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Changes.Responses;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using NodaTime.Text;
    using Swashbuckle.AspNetCore.Filters;

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

        /// <summary>
        /// Request an archive of the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="context">The database context to query data with.</param>
        /// <response code="200">Returned if the road registry activity can be downloaded.</response>
        /// <response code="500">Returned if the road registry activity can not be downloaded due to an unforeseen server error.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ChangeFeedResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get([FromServices] ShapeContext context)
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
