namespace RoadRegistry.BackOffice.Handlers.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using FeatureToggles;

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
    private readonly IZipArchiveBeforeFeatureCompareValidator _beforeFeatureCompareValidator;
    private readonly UseUploadZipArchiveValidationFeatureToggle _uploadZipArchiveValidationFeatureToggle;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IZipArchiveAfterFeatureCompareValidator _afterFeatureCompareValidator;

    public UploadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkUploadsBlobClient client,
        IZipArchiveBeforeFeatureCompareValidator beforeFeatureCompareValidator,
        IZipArchiveAfterFeatureCompareValidator afterFeatureCompareValidator,
        UseUploadZipArchiveValidationFeatureToggle uploadZipArchiveValidationFeatureToggle,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        ILogger<UploadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _beforeFeatureCompareValidator = beforeFeatureCompareValidator ?? throw new ValidatorNotFoundException(nameof(beforeFeatureCompareValidator));
        _afterFeatureCompareValidator = afterFeatureCompareValidator ?? throw new ValidatorNotFoundException(nameof(afterFeatureCompareValidator));
        _uploadZipArchiveValidationFeatureToggle = uploadZipArchiveValidationFeatureToggle ?? throw new ArgumentNullException(nameof(uploadZipArchiveValidationFeatureToggle));
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException();
        }

        await using var readStream = request.Archive.ReadStream;
        ArchiveId archiveId = new(Guid.NewGuid().ToString("N"));

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        if (_uploadZipArchiveValidationFeatureToggle.FeatureEnabled)
            await ValidateAndUploadAndDispatchCommand(request, readStream, archiveId, metadata, cancellationToken);
        else
            await UploadAndDispatchCommand(request, readStream, archiveId, metadata, cancellationToken);

        return new UploadExtractResponse(archiveId);
    }

    private async Task UploadAndDispatchCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
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
            ArchiveId = archiveId.ToString(),
            UseZipArchiveFeatureCompareTranslator = request.UseZipArchiveFeatureCompareTranslator
        });
        await _roadNetworkCommandQueue.Write(command, cancellationToken);

        _logger.LogInformation("Command queued {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), archiveId);
    }

    private async Task ValidateAndUploadAndDispatchCommand(UploadExtractRequest request, Stream readStream, ArchiveId archiveId, Metadata metadata, CancellationToken cancellationToken)
    {
        var entity = RoadNetworkChangesArchive.Upload(archiveId, readStream);

        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            IZipArchiveValidator validator = request.UseZipArchiveFeatureCompareTranslator ? _beforeFeatureCompareValidator : _afterFeatureCompareValidator;
            var problems = entity.ValidateArchiveUsing(archive, validator);

            var fileProblems = problems.OfType<FileError>().ToArray();
            if (fileProblems.Any())
            {
                throw new ZipArchiveValidationException(problems);
            }

            await UploadAndDispatchCommand(request, readStream, archiveId, metadata, cancellationToken);
        }
    }
}
