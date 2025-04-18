namespace RoadRegistry.BackOffice.Handlers.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extensions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Exceptions;
using FeatureCompare;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using ZipArchiveWriters.Cleaning;
using ContentType = Be.Vlaanderen.Basisregisters.BlobStore.ContentType;

public class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("binary/octet-stream"),
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkUploadsBlobClient _client;
    private readonly EditorContext _editorContext;
    private readonly IZipArchiveBeforeFeatureCompareValidatorFactory _beforeFeatureCompareValidatorFactory;
    private readonly IBeforeFeatureCompareZipArchiveCleanerFactory _beforeFeatureCompareZipArchiveCleanerFactory;
    private readonly ITransactionZoneZipArchiveReader _transactionZoneZipArchiveReader;

    public UploadExtractRequestHandler(
        RoadNetworkUploadsBlobClient client,
        ITransactionZoneZipArchiveReader transactionZoneZipArchiveReader,
        EditorContext editorContext,
        IZipArchiveBeforeFeatureCompareValidatorFactory beforeFeatureCompareValidatorFactory,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IBeforeFeatureCompareZipArchiveCleanerFactory beforeFeatureCompareZipArchiveCleanerFactory,
        ILogger<UploadExtractRequestHandler> logger) : base(roadNetworkCommandQueue, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _transactionZoneZipArchiveReader = transactionZoneZipArchiveReader.ThrowIfNull();
        _editorContext = editorContext ?? throw new EditorContextNotFoundException(nameof(editorContext));
        _beforeFeatureCompareValidatorFactory = beforeFeatureCompareValidatorFactory ?? throw new ValidatorNotFoundException(nameof(beforeFeatureCompareValidatorFactory));
        _beforeFeatureCompareZipArchiveCleanerFactory = beforeFeatureCompareZipArchiveCleanerFactory.ThrowIfNull();
    }

    protected override async Task<UploadExtractResponse> InnerHandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException(request.Archive.ContentType);
        }

        var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        await ValidateAndUploadAndQueueCommand(request, archiveId, metadata, cancellationToken);

        return new UploadExtractResponse(archiveId);
    }

    private async Task CleanArchive(Stream archiveStream, string zipArchiveWriterVersion, CancellationToken cancellationToken)
    {
        try
        {
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

            try
            {
                var cleaner = _beforeFeatureCompareZipArchiveCleanerFactory.Create(zipArchiveWriterVersion);
                await cleaner.CleanAsync(archive, cancellationToken);
            }
            catch
            {
                // ignore exceptions, let the validation handle it
            }
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }
    }

    private async Task ValidateAndUploadAndQueueCommand(UploadExtractRequest request, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
    {
        try
        {
            await using var archiveStream = await request.Archive.ReadStream.CopyToNewMemoryStreamAsync(cancellationToken);

            var transactionZone = ReadTransactionZone(archiveStream);
            var downloadId = transactionZone.DownloadId;

            var download = await _editorContext.ExtractDownloads.FindAsync([downloadId.ToGuid()], cancellationToken);
            if (download is null)
            {
                throw new ExtractDownloadNotFoundException(downloadId);
            }

            if (download.IsInformative)
            {
                throw new ExtractRequestMarkedInformativeException(downloadId);
            }

            await CleanArchive(archiveStream, download.ZipArchiveWriterVersion, cancellationToken);

            archiveStream.Position = 0;
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, true);

            var beforeFeatureCompareValidator = _beforeFeatureCompareValidatorFactory.Create(download.ZipArchiveWriterVersion);
            var problems = await beforeFeatureCompareValidator.ValidateAsync(archive, ZipArchiveMetadata.Empty, cancellationToken);
            problems.ThrowIfError();

            var externalExtractRequestId = new ExternalExtractRequestId(download.ExternalRequestId);
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            await UploadAndQueueCommand(request, archiveStream, extractRequestId, downloadId, archiveId, download.ZipArchiveWriterVersion, metadata, cancellationToken);
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }
    }

    private TransactionZoneDetails ReadTransactionZone(Stream archiveStream)
    {
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, true);

        return _transactionZoneZipArchiveReader.Read(archive);
    }

    private async Task UploadAndQueueCommand(
        UploadExtractRequest request,
        Stream archiveStream,
        ExtractRequestId extractRequestId,
        DownloadId downloadId,
        ArchiveId archiveId,
        string zipArchiveWriterVersion,
        Metadata metadata,
        CancellationToken cancellationToken)
    {
        archiveStream.Position = 0;

        await _client.CreateBlobAsync(
            new BlobName(archiveId.ToString()),
            metadata,
            ContentType.Parse("application/zip"),
            archiveStream,
            cancellationToken
        );

        var command = new Command(new UploadRoadNetworkChangesArchive
        {
            ExtractRequestId = extractRequestId,
            DownloadId = downloadId,
            ArchiveId = archiveId,
            TicketId = request.TicketId,
            ZipArchiveWriterVersion = zipArchiveWriterVersion
        }).WithProvenanceData(request.ProvenanceData);

        await Queue(command, cancellationToken);

        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), archiveId);
    }
}
