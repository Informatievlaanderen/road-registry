namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections;

using System;
using NodaTime;
using NodaTime.Text;

public static class LocalDateTimeTranslator
{
    private static readonly DateTimeZone LocalTimeZone =
        DateTimeZoneProviders.Tzdb["Europe/Brussels"];

    public static DateTime TranslateFromWhen(string value)
    {
        return new ZonedDateTime(InstantPattern.ExtendedIso.Parse(value).Value, LocalTimeZone)
            .ToDateTimeUnspecified();
    }

    public static string TranslateToWhen(DateTime value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
    }
    public static string TranslateToWhen(DateTimeOffset value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
    }
}
