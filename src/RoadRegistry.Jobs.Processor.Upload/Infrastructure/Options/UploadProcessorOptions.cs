namespace RoadRegistry.Jobs.Processor.Upload.Infrastructure.Options;

using BackOffice;

public class UploadProcessorOptions : IHasConfigurationKey
{
    public bool AlwaysRunning { get; set; }
    public int ConsumerDelaySeconds { get; set; } = 10;

    public string GetConfigurationKey()
    {
        return null;
    }
}
