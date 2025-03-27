namespace RoadRegistry.Product.PublishHost.Infrastructure.Configurations;

public class MetadataCenterOptions
{
    public string FullIdentifier { get; set; }
    public string BaseUrl { get; set; }

    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TokenEndPoint { get; set; }
}
