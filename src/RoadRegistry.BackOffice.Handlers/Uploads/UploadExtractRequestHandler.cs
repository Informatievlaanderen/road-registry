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
using ZipArchiveWriters;
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
    private readonly IBeforeFeatureCompareZipArchiveCleaner _beforeFeatureCompareZipArchiveCleaner;
    private readonly ITransactionZoneZipArchiveReader _transactionZoneZipArchiveReader;

    public UploadExtractRequestHandler(
        RoadNetworkUploadsBlobClient client,
        ITransactionZoneZipArchiveReader transactionZoneZipArchiveReader,
        EditorContext editorContext,
        IZipArchiveBeforeFeatureCompareValidatorFactory beforeFeatureCompareValidatorFactory,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IBeforeFeatureCompareZipArchiveCleaner beforeFeatureCompareZipArchiveCleaner,
        ILogger<UploadExtractRequestHandler> logger) : base(roadNetworkCommandQueue, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _transactionZoneZipArchiveReader = transactionZoneZipArchiveReader.ThrowIfNull();
        _editorContext = editorContext ?? throw new EditorContextNotFoundException(nameof(editorContext));
        _beforeFeatureCompareValidatorFactory = beforeFeatureCompareValidatorFactory ?? throw new ValidatorNotFoundException(nameof(beforeFeatureCompareValidatorFactory));
        _beforeFeatureCompareZipArchiveCleaner = beforeFeatureCompareZipArchiveCleaner.ThrowIfNull();
    }

    protected override async Task<UploadExtractResponse> InnerHandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException(request.Archive.ContentType);
        }

        await using var readStream = await CleanArchive(request.Archive.ReadStream, cancellationToken);

        var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        await ValidateAndUploadAndQueueCommand(request, readStream, archiveId, metadata, cancellationToken);

        return new UploadExtractResponse(archiveId);
    }

    private async Task<Stream> CleanArchive(Stream readStream, CancellationToken cancellationToken)
    {
        var writeStream = await readStream.CopyToNewMemoryStream(cancellationToken);

        try
        {
            using var archive = new ZipArchive(writeStream, ZipArchiveMode.Update, true);

            CleanResult cleanResult;
            try
            {
                cleanResult = await _beforeFeatureCompareZipArchiveCleaner.CleanAsync(archive, cancellationToken);
            }
            catch
            {
                // ignore exceptions, let the validation handle it
                cleanResult = CleanResult.NotApplicable;
            }

            if (cleanResult != CleanResult.Changed)
            {
                readStream.Position = 0;
                return readStream;
            }
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }

        writeStream.Position = 0;
        return writeStream;
    }

    private async Task ValidateAndUploadAndQueueCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
    {
        try
        {
            using var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false);

            var transactionZone = _transactionZoneZipArchiveReader.Read(archive);
            var downloadId = transactionZone.DownloadId;

            var download = await _editorContext.ExtractDownloads.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);
            if (download is null)
            {
                throw new ExtractDownloadNotFoundException(downloadId);
            }
            if (download.IsInformative)
            {
                throw new ExtractRequestMarkedInformativeException(downloadId);
            }

            var beforeFeatureCompareValidator = _beforeFeatureCompareValidatorFactory.Create(download.ZipArchiveWriterVersion);
            var problems = await beforeFeatureCompareValidator.ValidateAsync(archive, ZipArchiveMetadata.Empty, cancellationToken);
            problems.ThrowIfError();

            var externalExtractRequestId = new ExternalExtractRequestId(download.ExternalRequestId);
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            await UploadAndQueueCommand(request, readStream, extractRequestId, downloadId, archiveId, download.ZipArchiveWriterVersion, metadata, cancellationToken);
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }
    }

    private async Task UploadAndQueueCommand(
        UploadExtractRequest request,
        Stream readStream,
        ExtractRequestId extractRequestId,
        DownloadId downloadId,
        ArchiveId archiveId,
        string zipArchiveWriterVersion,
        Metadata metadata,
        CancellationToken cancellationToken)
    {
        readStream.Position = 0;

        await _client.CreateBlobAsync(
            new BlobName(archiveId.ToString()),
            metadata,
            ContentType.Parse("application/zip"),
            readStream,
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
