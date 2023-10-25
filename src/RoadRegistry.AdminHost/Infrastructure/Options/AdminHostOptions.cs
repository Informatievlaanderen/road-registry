namespace RoadRegistry.AdminHost.Infrastructure.Options;

using BackOffice;

public class AdminHostOptions : IHasConfigurationKey
{
    public bool AlwaysRunning { get; set; }

    public string GetConfigurationKey()
    {
        return null;
    }
}
