namespace RoadRegistry.Product.PublishHost.CloudStorageClients
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using Infrastructure.Configurations;
    using Microsoft.Extensions.Options;

    public class AzureBlobClient
    {
        private readonly AzureBlobOptions _options;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobClient(IOptions<AzureBlobOptions> options, BlobServiceClient client)
        {
            _options = options.Value;
            _containerClient = client.GetBlobContainerClient(_options.ContainerName);
            if (_options.IsAzurite)
            {
                _containerClient.CreateIfNotExists(PublicAccessType.BlobContainer);
            }
        }

        public async Task UploadBlobAsync(
            MemoryStream sourceStream,
            CancellationToken cancellationToken)
        {
            sourceStream.Seek(0, SeekOrigin.Begin);
            var blobName = GetBlobName();
            var blobClient = _containerClient.GetBlockBlobClient(blobName);
            await blobClient.UploadAsync(
                sourceStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/octet-stream"
                    }
                }, cancellationToken);
        }

        private string GetBlobName()
        {
            var isTest = _options.IsTest;
            var blobName = "Wegenregister.zip";

            return isTest ? $"9449/{blobName}" : $"1373/{blobName}";
        }
    }
}
