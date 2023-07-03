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

public record ChangeFeedEntry(long Id, string Title, string Type, string Day, string Month, string TimeOfDay);

public record ChangeFeedEntryContent(long Id, string Type, object Content);

public record ChangeFeedResponse(ChangeFeedEntry[] Entries);
