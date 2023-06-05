namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

/// <summary>
///     Post upload extract controller
/// </summary>
/// <exception cref="BlobClientNotFoundException"></exception>
/// <exception cref="EditorContextNotFoundException"></exception>
/// <exception cref="DownloadExtractNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
public class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly EditorContext _context;

    public UploadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        EditorContext context,
        ILogger<UploadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _context = context ?? throw new EditorContextNotFoundException(nameof(context));
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsedContentType) || !SupportedContentTypes.Contains(parsedContentType))
        {
            throw new UnsupportedMediaTypeException();
        }

        if (request.DownloadId is null)
        {
            throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");
        }

        if (!Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            throw new UploadExtractNotFoundException($"Could not upload the extract with filename {request.Archive.FileName}");
        }

        var extractRequest = await _context.ExtractRequests.FindAsync(new object[] { parsedDownloadId }, cancellationToken)
                             ?? throw new ExtractDownloadNotFoundException(new DownloadId(parsedDownloadId));

        if (extractRequest.IsInformative)
        {
            throw new ExtractRequestMarkedInformativeException(new DownloadId(parsedDownloadId));
        }

        var download = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken)
                       ?? throw new ExtractDownloadNotFoundException(new DownloadId(parsedDownloadId));

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

        var message = new Command(
            new UploadRoadNetworkExtractChangesArchive
            {
                RequestId = download.RequestId,
                DownloadId = download.DownloadId,
                UploadId = uploadId.ToGuid(),
                ArchiveId = archiveId.ToString(),
                UseZipArchiveFeatureCompareTranslator = request.UseZipArchiveFeatureCompareTranslator
            });

        await Dispatcher(message, cancellationToken);

        return new UploadExtractResponse(uploadId);
    }
}
