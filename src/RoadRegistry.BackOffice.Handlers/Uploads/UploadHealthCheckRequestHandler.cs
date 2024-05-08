namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Abstractions;
using Abstractions.Uploads;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using ContentType = Be.Vlaanderen.Basisregisters.BlobStore.ContentType;

public class UploadHealthCheckRequestHandler : EndpointRequestHandler<UploadHealthCheckRequest, UploadHealthCheckResponse>
{
    private readonly RoadNetworkUploadsBlobClient _client;

    public UploadHealthCheckRequestHandler(
        RoadNetworkUploadsBlobClient client,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        ILogger<UploadHealthCheckRequestHandler> logger) : base(roadNetworkCommandQueue, logger)
    {
        _client = client.ThrowIfNull();
    }

    public override async Task<UploadHealthCheckResponse> HandleAsync(UploadHealthCheckRequest request, CancellationToken cancellationToken)
    {
        var fileName = "healthcheck.bin";

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), fileName)
        );

        var blobName = new BlobName(fileName);

        if (await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            await _client.DeleteBlobAsync(blobName, cancellationToken);
        }

        await using var emptyFileStream = new MemoryStream();

        await _client.CreateBlobAsync(
            new BlobName(fileName),
            metadata,
            ContentType.Parse("application/octet-stream"),
            emptyFileStream,
            cancellationToken
        );

        var command = new Command(new CheckUploadHealth
        {
            TicketId = request.TicketId,
            FileName = fileName
        });
        await Queue(command, cancellationToken);

        return new UploadHealthCheckResponse();
    }
}
