namespace RoadRegistry.BackOffice.Api.Changes;

using System.Globalization;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Text;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("changefeed")]
[ApiExplorerSettings(GroupName = "Activiteiten")]
public partial class ChangeFeedController : ApiController
{
    private readonly LocalDatePattern _localMonthPattern;
    private readonly LocalTimePattern _localTimeOfDayPattern;
    private readonly DateTimeZone _localTimeZone;

    public ChangeFeedController(IMediator? mediator)
    {
        _localTimeZone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];
        _localMonthPattern = LocalDatePattern.Create("MMM", new CultureInfo("nl-BE"));
        _localTimeOfDayPattern = LocalTimePattern.CreateWithInvariantCulture("HH':'mm");
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
