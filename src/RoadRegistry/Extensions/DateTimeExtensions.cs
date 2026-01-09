namespace RoadRegistry.Extensions;

using System;
using NodaTime;

public static class DateTimeExtensions
{
    private static readonly DateTimeZone LocalTimeZone = DateTimeZoneProviders.Tzdb["Europe/Brussels"];

    public static DateTime ToBrusselsDateTime(this Instant value)
    {
        return new ZonedDateTime(value, LocalTimeZone).ToDateTimeUnspecified();
    }
}
