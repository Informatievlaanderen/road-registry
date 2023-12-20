namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;

public class SqsMessagesBlobClient : IBlobClient
{
    private readonly IBlobClient _client;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;

    public SqsMessagesBlobClient(IBlobClient client, SqsJsonMessageSerializer sqsJsonMessageSerializer)
    {
        _client = client.ThrowIfNull();
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
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
    
    public async Task CreateBlobMessageAsync<T>(BlobName name, Metadata metadata, ContentType contentType, T message,
        CancellationToken cancellationToken = default)
    {
        var json = _sqsJsonMessageSerializer.Serialize(message);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            await CreateBlobAsync(name, metadata, contentType, stream, cancellationToken);
        }
    }

    public async Task<object> GetBlobMessageAsync(BlobName name, CancellationToken cancellationToken = default)
    {
        var blob = await GetBlobAsync(name, cancellationToken);
        if (blob is null)
        {
            return null;
        }

        using (var blobStream = await blob.OpenAsync(cancellationToken))
        using (var memoryStream = new MemoryStream())
        {
            await blobStream.CopyToAsync(memoryStream, cancellationToken);

            var blobJsonMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
            return _sqsJsonMessageSerializer.Deserialize(blobJsonMessage);
        }
    }
}
