namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using System.IO.Compression;
using Abstractions.Configuration;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Editor.Schema;
using Editor.Schema.Extracts;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions;
using SqlStreamStore.Streams;
using UploadExtractFeatureCompareRequest = Abstractions.Extracts.UploadExtractFeatureCompareRequest;

/// <summary>
///     Post upload extract controller
/// </summary>
public class UploadExtractFeatureCompareRequestHandler : EndpointRequestHandler<UploadExtractFeatureCompareRequest, UploadExtractFeatureCompareResponse>
{
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly RoadNetworkFeatureCompareBlobClient _client;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;
    private readonly IZipArchiveBeforeFeatureCompareValidator _validator;
    private readonly IExtractUploadFailedEmailClient _emailClient;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;
    private readonly IRoadRegistryContext _roadRegistryContext;

    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly EditorContext _context;

    public UploadExtractFeatureCompareRequestHandler(
        FeatureCompareMessagingOptions messagingOptions,
        CommandHandlerDispatcher dispatcher,
        RoadNetworkFeatureCompareBlobClient client,
        ISqsQueuePublisher sqsQueuePublisher,
        IZipArchiveBeforeFeatureCompareValidator validator,
        IExtractUploadFailedEmailClient emailClient,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        EditorContext context,
        IRoadRegistryContext roadRegistryContext,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger)
        : base(dispatcher, logger)
    {
        _messagingOptions = messagingOptions;
        _client = client;
        _sqsQueuePublisher = sqsQueuePublisher;
        _validator = validator;
        _emailClient = emailClient;
        _roadNetworkEventWriter = roadNetworkEventWriter;
        _roadRegistryContext = roadRegistryContext;
        _context = context.ThrowIfNull();
    }

    public override async Task<UploadExtractFeatureCompareResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
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
        ArchiveId archiveId = new(Guid.NewGuid().ToString("N"));
        var uploadId = new UploadId(archiveId);

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        var extractRequestId = ExtractRequestId.FromString(download.RequestId);
        var extract = await _roadRegistryContext.RoadNetworkExtracts.Get(extractRequestId, cancellationToken);

        var upload = extract.Upload(new DownloadId(parsedDownloadId), uploadId, archiveId);
        
        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            var problems = await upload.ValidateArchiveUsing(archive, _validator, _emailClient, cancellationToken);
            if (problems.HasError())
            {
                throw new ZipArchiveValidationException(problems);
            }

            readStream.Position = 0;
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
                UploadId = uploadId,
                ArchiveId = archiveId
            };
            await _sqsQueuePublisher.CopyToQueue(_messagingOptions.RequestQueueUrl, message, new SqsQueueOptions { MessageGroupId = SqsFeatureCompare.MessageGroupId }, cancellationToken);

            await WriteRoadNetworkChangesArchiveUploadedToStore(extract, download, uploadId, cancellationToken);
        }

        return new UploadExtractFeatureCompareResponse(uploadId);
    }

    private async Task WriteRoadNetworkChangesArchiveUploadedToStore(RoadNetworkExtract extract, ExtractDownloadRecord download, UploadId uploadId, CancellationToken cancellationToken)
    {
        await _roadNetworkEventWriter.WriteAsync(new StreamName(download.ArchiveId), Guid.NewGuid(), ExpectedVersion.NoStream, new object[]
        {
            new RoadNetworkExtractChangesArchiveUploaded
            {
                ArchiveId = download.ArchiveId,
                Description = extract.Description,
                DownloadId = download.DownloadId,
                ExternalRequestId = download.ExternalRequestId,
                RequestId = download.RequestId,
                UploadId = uploadId
            }
        }, cancellationToken);
    }
}
