namespace RoadRegistry.Product.PublishHost.Infrastructure.Configurations;

public class AzureBlobOptions
{
    public string BaseUrl { get; set; }
    public string TenantId { get; set; }
    public string ClientKey { get; set; }
    public string ClientSecret { get; set; }
    public string ContainerName { get; set; }
    public string ConnectionString { get; set; }
    public string BlobDirectory { get; set; }
    public bool IsAzurite { get; set; }
    public bool Enabled { get; set; }
}
