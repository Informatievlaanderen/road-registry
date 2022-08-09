namespace RoadRegistry.BackOffice.Api.Configuration;

public class ExtractDownloadsOptions
{
    public int DefaultRetryAfter { get; set; } = 60;
    public int RetryAfterAverageWindowInDays { get; set; } = 30;
}
