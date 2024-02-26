namespace RoadRegistry.BackOffice.Handlers.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.FeatureCompare.Translators;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Exceptions;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureCompare.Readers;
using ZipArchiveWriters;
using ZipArchiveWriters.Cleaning;

/// <summary>Upload controller, post upload</summary>
/// <exception cref="BlobClientNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
public class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkUploadsBlobClient _client;
    private readonly EditorContext _editorContext;
    private readonly IZipArchiveBeforeFeatureCompareValidator _beforeFeatureCompareValidator;
    private readonly IBeforeFeatureCompareZipArchiveCleaner _beforeFeatureCompareZipArchiveCleaner;
    private readonly TransactionZoneFeatureCompareFeatureReader _transactionZoneFeatureReader;

    public UploadExtractRequestHandler(
        RoadNetworkUploadsBlobClient client,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        EditorContext editorContext,
        IZipArchiveBeforeFeatureCompareValidator beforeFeatureCompareValidator,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IBeforeFeatureCompareZipArchiveCleaner beforeFeatureCompareZipArchiveCleaner,
        ILogger<UploadExtractRequestHandler> logger) : base(roadNetworkCommandQueue, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _editorContext = editorContext ?? throw new EditorContextNotFoundException(nameof(editorContext));
        _beforeFeatureCompareValidator = beforeFeatureCompareValidator ?? throw new ValidatorNotFoundException(nameof(beforeFeatureCompareValidator));
        _beforeFeatureCompareZipArchiveCleaner = beforeFeatureCompareZipArchiveCleaner.ThrowIfNull();
        _transactionZoneFeatureReader = transactionZoneFeatureReader.ThrowIfNull();
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException();
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
        var writeStream = new MemoryStream();
        await readStream.CopyToAsync(writeStream, cancellationToken);
        writeStream.Position = 0;

        using (var archive = new ZipArchive(writeStream, ZipArchiveMode.Update, true))
        {
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

        writeStream.Position = 0;
        return writeStream;
    }

    private async Task UploadAndQueueCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
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
            ArchiveId = archiveId
        });
        await Queue(command, cancellationToken);

        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), archiveId);
    }

    private async Task ValidateAndUploadAndQueueCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
    {
        var entity = RoadNetworkChangesArchive.Upload(archiveId, readStream);

        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            var problems = await entity.ValidateArchiveUsing(archive, _beforeFeatureCompareValidator, cancellationToken);

            problems.ThrowIfError();

            var readerContext = new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
            var features = _transactionZoneFeatureReader.Read(archive, FeatureType.Change, ExtractFileName.Transactiezones, readerContext).Item1;
            var downloadId = features.Single().Attributes.DownloadId;

            var extractRequest = await _editorContext.ExtractRequests.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);
            if (extractRequest is null)
            {
                throw new ExtractDownloadNotFoundException(new DownloadId(downloadId));
            }
            if (extractRequest.IsInformative)
            {
                throw new ExtractRequestMarkedInformativeException(downloadId);
            }

            await UploadAndQueueCommand(request, readStream, archiveId, metadata, cancellationToken);
        }
    }
}
