namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Configuration;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using SqlStreamStore.Streams;

/// <summary>Upload controller, post upload</summary>
public class UploadExtractFeatureCompareRequestHandler : EndpointRequestHandler<UploadExtractFeatureCompareRequest, UploadExtractFeatureCompareResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly RoadNetworkFeatureCompareBlobClient _client;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;
    private readonly IZipArchiveBeforeFeatureCompareValidator _validator;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;

    public UploadExtractFeatureCompareRequestHandler(
        FeatureCompareMessagingOptions messagingOptions,
        CommandHandlerDispatcher dispatcher,
        RoadNetworkFeatureCompareBlobClient client,
        ISqsQueuePublisher sqsQueuePublisher,
        IZipArchiveBeforeFeatureCompareValidator validator,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger) : base(dispatcher, logger)
    {
        _messagingOptions = messagingOptions;
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _validator = validator ?? throw new ValidatorNotFoundException(nameof(validator));
        _roadNetworkEventWriter = roadNetworkEventWriter;
        _sqsQueuePublisher = sqsQueuePublisher ?? throw new SqsQueuePublisherNotFoundException(nameof(sqsQueuePublisher));
    }

    public override async Task<UploadExtractFeatureCompareResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
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

        var entity = RoadNetworkChangesArchive.Upload(archiveId, readStream);

        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            var problems = entity.ValidateArchiveUsing(archive, _validator);

            var fileProblems = problems.OfType<FileError>();
            if (fileProblems.Any())
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
            
            var message = new UploadRoadNetworkChangesArchive
            {
                ArchiveId = archiveId.ToString()
            };
            await _sqsQueuePublisher.CopyToQueue(_messagingOptions.RequestQueueUrl, message, new SqsQueueOptions { MessageGroupId = SqsFeatureCompare.MessageGroupId }, cancellationToken);

            await WriteRoadNetworkChangesArchiveUploadedToStore(entity, cancellationToken);
        }

        return new UploadExtractFeatureCompareResponse(archiveId);
    }
    
    private async Task WriteRoadNetworkChangesArchiveUploadedToStore(RoadNetworkChangesArchive archive, CancellationToken cancellationToken)
    {
        await _roadNetworkEventWriter.WriteAsync(new StreamName(archive.Id), Guid.NewGuid(), ExpectedVersion.NoStream, new object[]
        {
            new RoadNetworkChangesArchiveUploaded { ArchiveId = archive.Id, Description = archive.Description }
        }, cancellationToken);
    }
}
