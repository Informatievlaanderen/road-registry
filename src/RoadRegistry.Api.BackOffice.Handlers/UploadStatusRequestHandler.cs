using RoadRegistry.BackOffice.Framework;

namespace RoadRegistry.Api.BackOffice.Handlers
{
    using System.Net.Http.Headers;
    using Amazon.Runtime.Internal.Util;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Microsoft.Extensions.Logging;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Uploads;

    internal class UploadStatusRequestHandler : EndpointRequestHandler<UploadStatusRequest, UploadStatusResponse>,
        IRequestExceptionHandler<UploadStatusRequest, UploadStatusResponse, UploadStatusNotFoundException>
    {
        private readonly ILogger<UploadStatusRequestHandler> _logger;
        private readonly RoadNetworkUploadsBlobClient _client;

        public UploadStatusRequestHandler(CommandHandlerDispatcher dispatcher, RoadNetworkUploadsBlobClient client) : base(dispatcher)
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

            var filename = metadata.Length == 1
                ? metadata[0].Value
                : archiveId + ".zip";

            return new UploadStatusResponse(
                new FileCallbackResult(
                    new MediaTypeHeaderValue("application/zip"),
                    async (stream, actionContext) =>
                    {
                        await using var blobStream = await blob.OpenAsync(actionContext.HttpContext.RequestAborted);
                        await blobStream.CopyToAsync(stream, actionContext.HttpContext.RequestAborted);
                    })
                {
                    FileDownloadName = filename
                }
            );
        }
    }
}
