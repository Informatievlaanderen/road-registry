namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Editor.Schema;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

/// <summary>
///     Post upload extract controller
/// </summary>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="EditorContextNotFoundException"></exception>
/// <exception cref="DownloadExtractNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
public class UploadExtractFeatureCompareRequestHandler : EndpointRequestHandler<UploadExtractFeatureCompareRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly EditorContext _context;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;

    public UploadExtractFeatureCompareRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        EditorContext context,
        ISqsQueuePublisher sqsQueuePublisher,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new UploadExtractBlobClientNotFoundException(nameof(client));
        _context = context ?? throw new EditorContextNotFoundException(nameof(context));
        _sqsQueuePublisher = sqsQueuePublisher ?? throw new SqsQueuePublisherNotFoundException(nameof(sqsQueuePublisher));
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null) throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");

        if (Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            if (!ContentType.TryParse(request.Archive.ContentType, out var parsedContentType) || !SupportedContentTypes.Contains(parsedContentType))
                throw new UnsupportedMediaTypeException();

            var download = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken)
                           ?? throw new ExtractDownloadNotFoundException(DownloadId.Parse(parsedDownloadId.ToString()));

            await using var readStream = request.Archive.ReadStream;

            var uploadId = new UploadId(Guid.NewGuid());
            var archiveId = new ArchiveId(uploadId.ToString());
            var metadata = Metadata.None;

            await _client.CreateBlobAsync(
                new BlobName(archiveId.ToString()),
                metadata,
                ContentType.Parse("application/zip"),
                readStream,
                cancellationToken
            );

            var message = new UploadRoadNetworkExtractChangesArchive
            {
                RequestId = download.RequestId,
                DownloadId = download.DownloadId,
                UploadId = uploadId.ToGuid(),
                ArchiveId = archiveId.ToString()
            };

            await _sqsQueuePublisher.CopyToQueue(SqsQueueName.FeatureCompare.RequestQueue, message, new SqsQueueOptions { MessageGroupId = SqsFeatureCompare.MessageGroupId }, cancellationToken);

            return new UploadExtractResponse(uploadId);
        }

        throw new UploadExtractNotFoundException($"Could not upload the extract with filename {request.Archive.FileName}");
    }
}
