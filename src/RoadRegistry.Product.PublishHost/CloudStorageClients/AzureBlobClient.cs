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

        private string GetBlobName()
        {
            var isTest = _options.IsTest;
            var blobName = "Wegenregister.zip"; //TODO-pr update

            return isTest ? $"31086/{blobName}" : $"10142/{blobName}"; //TODO-pr update IDs
        }

        public async Task UploadBlobAsync(
            MemoryStream sourceStream,
            CancellationToken cancellationToken = default)
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

        // public async Task<IEnumerable<Tuple<string, string, long?>>> ListBlobsAsync(
        //     CancellationToken cancellationToken = default)
        // {
        //     var blobItems = new List<Tuple<string, string, long?>>();
        //     try
        //     {
        //         await foreach (var blobItem in _containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        //         {
        //             _logger.LogDebug($"Blob name: {blobItem.Name}, Blob type: {blobItem.Properties.BlobType}");
        //             var name = blobItem.Name;
        //             var type = blobItem.Properties.ContentType;
        //             var size = blobItem.Properties.ContentLength;
        //
        //             blobItems.Add(Tuple.Create(name, type, size));
        //         }
        //     }
        //     catch (RequestFailedException ex)
        //     {
        //         _logger.LogError($"Error listing blobs: {ex.Status}:{ex.ErrorCode} - {ex.Message}", ex);
        //     }
        //
        //     return blobItems;
        // }
        //
        //
        // public async Task<byte[]?> DownloadBlobAsync(string blobName, CancellationToken cancellationToken = default)
        // {
        //     try
        //     {
        //         var blobClient = _containerClient.GetBlobClient(blobName);
        //         var response = await blobClient.DownloadContentAsync(cancellationToken);
        //         var zipAsBytes = response.Value.Content.ToArray();
        //         return zipAsBytes;
        //     }
        //     catch (RequestFailedException ex)
        //     {
        //         _logger.LogError($"Error listing blobs: {ex.Status}:{ex.ErrorCode} - {ex.Message}", ex);
        //     }
        //
        //     return null;
        // }
    }
}
