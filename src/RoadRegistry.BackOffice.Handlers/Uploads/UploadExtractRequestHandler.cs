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
    private readonly UseUploadZipArchiveValidationFeatureToggle _uploadZipArchiveValidationFeatureToggle;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IZipArchiveAfterFeatureCompareValidator _afterFeatureCompareValidator;
    private readonly TransactionZoneFeatureCompareFeatureReader _transactionZoneFeatureReader;

    public UploadExtractRequestHandler(
        RoadNetworkUploadsBlobClient client,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        EditorContext editorContext,
        IZipArchiveBeforeFeatureCompareValidator beforeFeatureCompareValidator,
        IZipArchiveAfterFeatureCompareValidator afterFeatureCompareValidator,
        UseUploadZipArchiveValidationFeatureToggle uploadZipArchiveValidationFeatureToggle,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        FileEncoding encoding,
        UseCleanZipArchiveFeatureToggle useCleanZipArchiveFeatureToggle,
        ILogger<UploadExtractRequestHandler> logger) : base(roadNetworkCommandQueue, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _editorContext = editorContext ?? throw new EditorContextNotFoundException(nameof(editorContext));
        _beforeFeatureCompareValidator = beforeFeatureCompareValidator ?? throw new ValidatorNotFoundException(nameof(beforeFeatureCompareValidator));
        _afterFeatureCompareValidator = afterFeatureCompareValidator ?? throw new ValidatorNotFoundException(nameof(afterFeatureCompareValidator));
        _uploadZipArchiveValidationFeatureToggle = uploadZipArchiveValidationFeatureToggle ?? throw new ArgumentNullException(nameof(uploadZipArchiveValidationFeatureToggle));
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _transactionZoneFeatureReader = transactionZoneFeatureReader ?? throw new ArgumentNullException(nameof(transactionZoneFeatureReader));
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException();
        }
        
        var readStream = request.Archive.ReadStream;
        ArchiveId archiveId = new(Guid.NewGuid().ToString("N"));
        
        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        if (_uploadZipArchiveValidationFeatureToggle.FeatureEnabled)
            await ValidateAndUploadAndQueueCommand(request, readStream, archiveId, metadata, cancellationToken);
        else
            await UploadAndQueueCommand(request, readStream, archiveId, metadata, cancellationToken);

        return new UploadExtractResponse(archiveId);
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
            ArchiveId = archiveId,
            UseZipArchiveFeatureCompareTranslator = request.UseZipArchiveFeatureCompareTranslator
        });
        await Queue(command, cancellationToken);

        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), archiveId);
    }

    private async Task ValidateAndUploadAndQueueCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
    {
        var entity = RoadNetworkChangesArchive.Upload(archiveId, readStream);

        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            IZipArchiveValidator validator = request.UseZipArchiveFeatureCompareTranslator ? _beforeFeatureCompareValidator : _afterFeatureCompareValidator;
            var problems = entity.ValidateArchiveUsing(archive, validator);

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
