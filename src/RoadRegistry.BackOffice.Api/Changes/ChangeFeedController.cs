namespace RoadRegistry.BackOffice.Api.Changes;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("changefeed")]
[ApiExplorerSettings(GroupName = "ChangeFeed")]
[ApiKeyAuth("Road")]
public class ChangeFeedController : ControllerBase
{
    private readonly LocalDatePattern _localMonthPattern;
    private readonly LocalTimePattern _localTimeOfDayPattern;
    private readonly DateTimeZone _localTimeZone;

    public ChangeFeedController()
    {
        _localTimeZone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];
        _localMonthPattern = LocalDatePattern.Create("MMM", new CultureInfo("nl-BE"));
        _localTimeOfDayPattern = LocalTimePattern.CreateWithInvariantCulture("HH':'mm");
    }

    [HttpGet("entry/{id}/content")]
    public async Task<IActionResult> GetContent([FromServices] EditorContext context, long id)
    {
        var entry = await context
            .RoadNetworkChanges
            .SingleOrDefaultAsync(change => change.Id == id, HttpContext.RequestAborted);
        if (entry == null) return NotFound();

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
            nameof(RoadNetworkChangesAccepted) + ":v2" => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkChangesBasedOnArchiveAcceptedEntry)),
            nameof(RoadNetworkChangesRejected) => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkChangesBasedOnArchiveRejectedEntry)),
            nameof(NoRoadNetworkChanges) => JsonConvert.DeserializeObject(entry.Content,
                typeof(NoRoadNetworkChangesBasedOnArchiveEntry)),
            nameof(RoadNetworkExtractChangesArchiveUploaded) => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkExtractChangesArchiveUploadedEntry)),
            nameof(RoadNetworkExtractChangesArchiveAccepted) => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkExtractChangesArchiveAcceptedEntry)),
            nameof(RoadNetworkExtractChangesArchiveRejected) => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkExtractChangesArchiveRejectedEntry)),
            nameof(RoadNetworkExtractDownloadBecameAvailable) => JsonConvert.DeserializeObject(entry.Content,
                typeof(RoadNetworkExtractDownloadBecameAvailableEntry)),
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

    [HttpGet("head")]
    public async Task<IActionResult> GetHead(
        [FromQuery(Name = "MaxEntryCount")] string[] maxEntryCountValue,
        [FromServices] EditorContext context)
    {
        if (maxEntryCountValue.Length == 0) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });

        if (maxEntryCountValue.Length != 1) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });

        if (!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount)) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });

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
                    TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay)
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
    public async Task<IActionResult> GetNext(
        [FromQuery(Name = "AfterEntry")] string[] afterEntryValue,
        [FromQuery(Name = "MaxEntryCount")] string[] maxEntryCountValue,
        [FromServices] EditorContext context)
    {
        if (afterEntryValue.Length == 0) throw new ValidationException(new[] { new ValidationFailure("AfterEntry", "AfterEntry query string parameter is missing.") });

        if (maxEntryCountValue.Length == 0) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });

        if (afterEntryValue.Length != 1) throw new ValidationException(new[] { new ValidationFailure("AfterEntry", "AfterEntry query string parameter requires exactly 1 value.") });

        if (maxEntryCountValue.Length != 1) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });

        if (!long.TryParse(afterEntryValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var afterEntry)) throw new ValidationException(new[] { new ValidationFailure("AfterEntry", "AfterEntry query string parameter value must be an integer.") });

        if (!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount)) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });

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
                    TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay)
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
    public async Task<IActionResult> GetPrevious(
        [FromQuery(Name = "BeforeEntry")] string[] beforeEntryValue,
        [FromQuery(Name = "MaxEntryCount")] string[] maxEntryCountValue,
        [FromServices] EditorContext context)
    {
        if (beforeEntryValue.Length == 0) throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter is missing.") });

        if (maxEntryCountValue.Length == 0) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });

        if (beforeEntryValue.Length != 1) throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter requires exactly 1 value.") });

        if (maxEntryCountValue.Length != 1) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });

        if (!long.TryParse(beforeEntryValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var beforeEntry)) throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter value must be an integer.") });

        if (!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount)) throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });

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
                    TimeOfDay = _localTimeOfDayPattern.Format(localWhen.TimeOfDay)
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
}