namespace RoadRegistry.StreetName;

using RoadRegistry.BackOffice;

public class StreetNameRegistryOptions : IHasConfigurationKey
{
    public string StreetNameRegistryBaseUrl { get; set; }

    public string GetConfigurationKey()
    {
        return "StreetNameRegistryOptions";
    }
}
