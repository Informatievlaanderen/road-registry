namespace RoadRegistry.BackOffice.Api.Handlers.Files
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Files;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using MediatR;
    using Microsoft.Net.Http.Headers;

    public sealed class DownloadFileRequestHandler : IRequestHandler<DownloadFileRequest, DownloadFileResponse>
    {
        private readonly IBlobClientFactory _blobClientFactory;

        public DownloadFileRequestHandler(IBlobClientFactory blobClientFactory)
        {
            _blobClientFactory = blobClientFactory;
        }

        public async Task<DownloadFileResponse> Handle(
            DownloadFileRequest request,
            CancellationToken cancellationToken)
        {
            var blobClient = _blobClientFactory.Create(request.BucketKey);

            if (!await blobClient.BlobExistsAsync(request.BlobName, cancellationToken))
            {
                throw new BlobNotFoundException(request.BlobName);
            }

            var blob = await blobClient.GetBlobAsync(request.BlobName, cancellationToken);

            return new DownloadFileResponse(
                request.FileName,
                new MediaTypeHeaderValue("application/zip"),
                async (stream, _) =>
                {
                    await using var blobStream = await blob!.OpenAsync(cancellationToken);
                    await blobStream.CopyToAsync(stream, cancellationToken);
                });
        }
    }
}
