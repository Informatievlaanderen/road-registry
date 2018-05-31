namespace Shaperon
{
    using System;

    internal static class DateTimeRounding
    {
        public static DateTime RoundToSeconds(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, 0, value.Kind);
        }

        public static DateTime? RoundToSeconds(this DateTime? value)
        {
            if(value.HasValue)
            {
                return new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, value.Value.Hour, value.Value.Minute, value.Value.Second, 0, value.Value.Kind);
            }
            return value;
        }
    }
}
