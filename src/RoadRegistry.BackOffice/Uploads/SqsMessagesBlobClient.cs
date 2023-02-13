namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Newtonsoft.Json;
using Infrastructure.Converters;

public class SqsMessagesBlobClient : IBlobClient
{
    private readonly IBlobClient _client;
    private readonly SqsOptions _sqsOptions;

    public SqsMessagesBlobClient(IBlobClient client, SqsOptions sqsOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sqsOptions = sqsOptions;
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

    private JsonSerializer CreateJsonSerializer()
    {
        var serializer = JsonSerializer.CreateDefault(_sqsOptions.JsonSerializerSettings);

        serializer.Converters.Add(new MultiLineStringWktConverter());
        serializer.Converters.Add(new RoadSegmentAccessRestrictionConverter());
        serializer.Converters.Add(new RoadSegmentLaneCountConverter());
        serializer.Converters.Add(new RoadSegmentLaneDirectionConverter());
        serializer.Converters.Add(new RoadSegmentMorphologyConverter());
        serializer.Converters.Add(new RoadSegmentStatusConverter());
        serializer.Converters.Add(new RoadSegmentSurfaceTypeConverter());
        serializer.Converters.Add(new RoadSegmentWidthConverter());

        return serializer;
    }

    public async Task CreateBlobMessageAsync<T>(BlobName name, Metadata metadata, ContentType contentType, T message,
        CancellationToken cancellationToken = default)
    {
        var serializer = CreateJsonSerializer();
        
        var sqsJsonMessage = RoadRegistrySqsJsonMessage.Create(message, serializer);
        var json = serializer.Serialize(sqsJsonMessage);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            await CreateBlobAsync(name, metadata, contentType, stream, cancellationToken);
        }
    }

    public async Task<object> GetBlobMessageAsync(BlobName name, CancellationToken cancellationToken = default)
    {
        var blob = await GetBlobAsync(name, cancellationToken);

        var serializer = CreateJsonSerializer();

        using (var blobStream = await blob.OpenAsync(cancellationToken))
        using (var memoryStream = new MemoryStream())
        {
            await blobStream.CopyToAsync(memoryStream, cancellationToken);

            var blobJsonMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
            var sqsJsonMessage = serializer.Deserialize<RoadRegistrySqsJsonMessage>(blobJsonMessage);

            return sqsJsonMessage?.Map(serializer);
        }
    }
}
