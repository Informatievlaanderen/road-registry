namespace RoadRegistry.BackOffice.Api.Configuration;

public class ExtractUploadsOptions
{
    public int DefaultRetryAfter { get; set; } = 60;
    public int RetryAfterAverageWindowInDays { get; set; } = 30;
}
