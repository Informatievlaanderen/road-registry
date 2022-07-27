using RoadRegistry.BackOffice.Framework;

namespace RoadRegistry.Api.BackOffice.Handlers
{
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Messages;
    using RoadRegistry.BackOffice.Uploads;

    internal class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>,
        IRequestExceptionHandler<UploadExtractRequest, UploadExtractResponse, UploadExtractNotFoundException>
    {
        private readonly RoadNetworkUploadsBlobClient _client;

        public UploadExtractRequestHandler(CommandHandlerDispatcher dispatcher, RoadNetworkUploadsBlobClient client) : base(dispatcher)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task Handle(
            UploadExtractRequest request,
            UploadExtractNotFoundException exception,
            RequestExceptionHandlerState<UploadExtractResponse> state,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
        {
            if (request.Archive is null)
                throw new UploadExtractNullException(nameof(request.Archive));

            if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
                throw new UnsupportedMediaTypeException();

            var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
            var metadata = Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), string.IsNullOrEmpty(request.Archive.FileName) ? archiveId + ".zip" : request.Archive.FileName));

            await using (var readStream = request.Archive.OpenReadStream())
            {

                await _client.CreateBlobAsync(
                    new BlobName(archiveId.ToString()),
                    metadata,
                    ContentType.Parse("application/zip"),
                    readStream,
                    cancellationToken
                );

                var message = new Command(
                    new UploadRoadNetworkChangesArchive
                    {
                        ArchiveId = archiveId.ToString()
                    });

                await Dispatcher(message, cancellationToken);
            }

            return new UploadExtractResponse(archiveId, metadata);
        }

        private static readonly ContentType[] SupportedContentTypes =
        {
            ContentType.Parse("application/zip"),
            ContentType.Parse("application/x-zip-compressed")
        };
    }
}
