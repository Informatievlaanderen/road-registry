namespace RoadRegistry.BackOffice.Abstractions;

public class ExtractUploadsOptions
{
    public int DefaultRetryAfter { get; set; } = 60;
    public int RetryAfterAverageWindowInDays { get; set; } = 30;
}
