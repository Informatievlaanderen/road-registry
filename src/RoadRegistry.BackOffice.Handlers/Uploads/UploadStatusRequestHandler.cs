namespace RoadRegistry.BackOffice.Handlers.Uploads;

using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Contracts.Uploads;
using Exceptions;
using Framework;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

internal class UploadStatusRequestHandler : EndpointRequestHandler<UploadStatusRequest, UploadStatusResponse>,
    IRequestExceptionHandler<UploadStatusRequest, UploadStatusResponse, UploadStatusNotFoundException>
{
    private readonly RoadNetworkUploadsBlobClient _client;

    public UploadStatusRequestHandler(CommandHandlerDispatcher dispatcher, RoadNetworkUploadsBlobClient client, ILogger<UploadStatusRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task Handle(
        UploadStatusRequest request,
        UploadStatusNotFoundException exception,
        RequestExceptionHandlerState<UploadStatusResponse> state,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Upload status could not be supplied! {Identifier}", request.Identifier);
        return Task.CompletedTask;
    }

    public override async Task<UploadStatusResponse> HandleAsync(UploadStatusRequest request, CancellationToken cancellationToken)
    {
        var archiveId = new ArchiveId(request.Identifier);
        var blobName = new BlobName(archiveId.ToString());

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
            throw new UploadStatusNotFoundException($"Could not find details about upload with name {blobName}");

        var blob = await _client.GetBlobAsync(blobName, cancellationToken);

        var metadata = blob.Metadata
            .Where(pair => pair.Key == new MetadataKey("filename"))
            .ToArray();

        var fileName = metadata.Length == 1
            ? metadata[0].Value
            : archiveId + ".zip";

        return new UploadStatusResponse(fileName, new MediaTypeHeaderValue("application/zip"), async (stream, ct) =>
        {
            await using var blobStream = await blob.OpenAsync(ct);
            await blobStream.CopyToAsync(stream, ct);
        });
    }
}
