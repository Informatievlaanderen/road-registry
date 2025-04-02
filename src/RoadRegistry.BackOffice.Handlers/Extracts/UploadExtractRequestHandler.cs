namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extensions;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    [
        ContentType.Parse("binary/octet-stream"),
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    ];

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

    protected override async Task<UploadExtractResponse> InnerHandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
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

        var download = await _context.ExtractDownloads.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);
        if (download is null)
        {
            throw new ExtractDownloadNotFoundException(downloadId);
        }

        var previousUpload = await _context.ExtractUploads.IncludeLocalSingleOrDefaultAsync(x => x.RequestId == download.RequestId, cancellationToken);
        if (previousUpload is not null)
        {
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException();
        }

        if (download.IsInformative)
        {
            throw new ExtractRequestMarkedInformativeException(downloadId);
        }

        await using var readStream = await request.Archive.ReadStream.CopyToNewMemoryStream(cancellationToken);

        try
        {
            using (new ZipArchive(readStream, ZipArchiveMode.Read, true))
            {
            }
            readStream.Position = 0;
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }

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
                TicketId = request.TicketId,
                ZipArchiveWriterVersion = download.ZipArchiveWriterVersion
            })
            .WithProvenanceData(request.ProvenanceData);

        await Dispatch(message, cancellationToken);

        return new UploadExtractResponse(uploadId);
    }
}
