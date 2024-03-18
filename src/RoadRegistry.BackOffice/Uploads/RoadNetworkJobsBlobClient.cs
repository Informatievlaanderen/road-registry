namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;

public class RoadNetworkJobsBlobClient : IBlobClient
{
    private readonly IBlobClient _client;

    public RoadNetworkJobsBlobClient(IBlobClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task<bool> BlobExistsAsync(BlobName name, CancellationToken cancellationToken = default)
    {
        return _client.BlobExistsAsync(name, cancellationToken);
    }

    public Task CreateBlobAsync(BlobName name, Metadata metadata, ContentType contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        return _client.CreateBlobAsync(name, metadata, contentType, content, cancellationToken);
    }

    public Task DeleteBlobAsync(BlobName name, CancellationToken cancellationToken = default)
    {
        return _client.DeleteBlobAsync(name, cancellationToken);
    }

    public Task<BlobObject> GetBlobAsync(BlobName name, CancellationToken cancellationToken = default)
    {
        return _client.GetBlobAsync(name, cancellationToken);
    }
}
