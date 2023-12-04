namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Exceptions;
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
    private readonly IExtractUploadFailedEmailClient _emailClient;
    private readonly EditorContext _context;

    public UploadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        IExtractUploadFailedEmailClient emailClient,
        EditorContext context,
        ILogger<UploadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _emailClient = emailClient;
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

        if (!DownloadId.TryParse(request.DownloadId, out var downloadId))
        {
            throw new InvalidGuidValidationException(nameof(request.DownloadId));
        }

        var extractRequest = await _context.ExtractRequests.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);
        if (extractRequest is null)
        {
            var ex = new ExtractDownloadNotFoundException(downloadId);
            await _emailClient.SendAsync(null, ex, cancellationToken);
            throw ex;
        }
        if (extractRequest.IsInformative)
        {
            var ex = new ExtractRequestMarkedInformativeException(downloadId);
            await _emailClient.SendAsync(extractRequest.Description, ex, cancellationToken);
            throw ex;
        }

        var download = await _context.ExtractDownloads.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);
        if (download is null)
        {
            var ex = new ExtractDownloadNotFoundException(downloadId);
            await _emailClient.SendAsync(null, ex, cancellationToken);
            throw ex;
        }

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
                UploadId = uploadId,
                ArchiveId = archiveId,
                UseZipArchiveFeatureCompareTranslator = request.UseZipArchiveFeatureCompareTranslator
            });

        await Dispatch(message, cancellationToken);

        return new UploadExtractResponse(uploadId);
    }
}
