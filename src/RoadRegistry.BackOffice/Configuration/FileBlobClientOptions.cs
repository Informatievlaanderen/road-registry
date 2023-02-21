namespace RoadRegistry.BackOffice.Configuration;

public class FileBlobClientOptions: IHasConfigurationKey
{
    public string Directory { get; set; }

    public string GetConfigurationKey()
    {
        return "FileBlobClientOptions";
    }
}
