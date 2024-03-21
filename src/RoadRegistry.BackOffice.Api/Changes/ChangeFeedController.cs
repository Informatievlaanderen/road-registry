namespace RoadRegistry.BackOffice.Api.Changes;

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("changefeed")]
[ApiExplorerSettings(GroupName = "Activiteiten")]
public partial class ChangeFeedController : ApiController
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

    private async Task<ChangeFeedEntry[]> GetChangeFeedEntries(EditorContext context, Func<IQueryable<RoadNetworkChange>, IQueryable<RoadNetworkChange>> queryFilter)
    {
        return (await queryFilter(context
                    .RoadNetworkChanges
                    .AsQueryable()
                )
                .Select(change => new
                {
                    change.Id,
                    change.Title,
                    change.Type,
                    change.When
                })
                .ToListAsync(HttpContext.RequestAborted)
            )
            .Select(change =>
            {
                var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                return new ChangeFeedEntry(change.Id, change.Title, change.Type, localWhen.Day.ToString("00"), _localMonthPattern.Format(localWhen.Date), _localTimeOfDayPattern.Format(localWhen.TimeOfDay));
            })
            .OrderByDescending(entry => entry.Id)
            .ToArray();
    }
}

public record ChangeFeedEntry
{
    public ChangeFeedEntry(long id, string title, string type, string day, string month, string timeOfDay)
    {
        Id = id;
        Title = title;
        Type = type;
        Day = day;
        Month = month?.Length > 3 ? month.Substring(0, 3) : month;
        TimeOfDay = timeOfDay;
    }

    public long Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string Day { get; set; }
    public string Month { get; set; }
    public string TimeOfDay { get; set; }
};

public record ChangeFeedEntryContent(long Id, string Type, object Content);

public record ChangeFeedResponse(ChangeFeedEntry[] Entries);
