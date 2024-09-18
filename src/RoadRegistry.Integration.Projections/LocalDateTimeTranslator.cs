namespace RoadRegistry.Integration.Projections;

using System;
using NodaTime;
using NodaTime.Text;

public static class LocalDateTimeTranslator
{
    private static readonly DateTimeZone LocalTimeZone =
        DateTimeZoneProviders.Tzdb["Europe/Brussels"];

    public static Instant TranslateFromWhen(string value)
    {
        return Instant.FromDateTimeOffset(new ZonedDateTime(InstantPattern.ExtendedIso.Parse(value).Value, LocalTimeZone)
            .ToDateTimeUnspecified());
    }

    public static Instant ToBelgianInstant(this DateTime value)
    {
        return Instant.FromDateTimeOffset(new DateTimeOffset(value));
    }
}
