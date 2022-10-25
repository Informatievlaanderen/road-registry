namespace RoadRegistry.Hosts.Metadata;

public class MetadataConfiguration
{
    public const string Section = "Metadata";
    public bool Enabled { get; set; }
    public string Id { get; set; }
    public string LoginUri { get; set; }
    public string Password { get; set; }
    public string Uri { get; set; }
    public string Username { get; set; }
}