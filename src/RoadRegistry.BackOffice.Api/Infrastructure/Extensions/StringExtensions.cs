namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

public static class StringExtensions
{
    public static string Format(this string template, params object[] values)
    {
        return string.Format(template, values);
    }
}