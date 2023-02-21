namespace RoadRegistry.BackOffice.Abstractions;

public class ExtractDownloadsOptions: IHasConfigurationKey
{
    public int DefaultRetryAfter { get; set; } = 60;
    public int RetryAfterAverageWindowInDays { get; set; } = 30;

    public string GetConfigurationKey()
    {
        return "ExtractDownloadsOptions";
    }
}
