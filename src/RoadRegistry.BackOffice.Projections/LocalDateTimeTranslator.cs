namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using NodaTime;
    using NodaTime.Text;

    internal static class LocalDateTimeTranslator
    {
        private static readonly DateTimeZone LocalTimeZone = 
            DateTimeZoneProviders.Tzdb["Europe/Brussels"];

        public static DateTime TranslateFromWhen(string value)
        {
            return new ZonedDateTime(InstantPattern.ExtendedIso.Parse(value).Value, LocalTimeZone)
                .ToDateTimeUnspecified();
        }
    }
}
