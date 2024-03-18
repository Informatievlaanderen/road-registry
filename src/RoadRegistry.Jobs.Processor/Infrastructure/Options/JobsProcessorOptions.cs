namespace RoadRegistry.Jobs.Processor.Infrastructure.Options;

using BackOffice;

public class JobsProcessorOptions : IHasConfigurationKey
{
    public int MaxJobLifeTimeInMinutes { get; set; }
    public bool AlwaysRunning { get; set; }
    public int ConsumerDelaySeconds { get; set; } = 10;

    public string GetConfigurationKey()
    {
        return null;
    }
}
