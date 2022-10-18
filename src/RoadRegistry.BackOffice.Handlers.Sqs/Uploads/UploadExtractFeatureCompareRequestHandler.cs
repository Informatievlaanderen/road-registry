namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

/// <summary>Upload controller, post upload</summary>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
public class UploadExtractFeatureCompareRequestHandler : EndpointRequestHandler<UploadExtractFeatureCompareRequest, UploadExtractFeatureCompareResponse>
{
    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;
    private readonly IZipArchiveBeforeFeatureCompareValidator _validator;

    public UploadExtractFeatureCompareRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        ISqsQueuePublisher sqsQueuePublisher,
        IZipArchiveBeforeFeatureCompareValidator validator,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new UploadExtractBlobClientNotFoundException(nameof(client));
        _validator = validator ?? throw new ValidatorNotFoundException(nameof(validator));
        _sqsQueuePublisher = sqsQueuePublisher ?? throw new SqsQueuePublisherNotFoundException(nameof(sqsQueuePublisher));
    }

    public override async Task<UploadExtractFeatureCompareResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
    {
        if (!ContentType.TryParse(request.Archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed)) throw new UnsupportedMediaTypeException();

        await using var readStream = request.Archive.ReadStream;
        ArchiveId archiveId = new(Guid.NewGuid().ToString("N"));

        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                string.IsNullOrEmpty(request.Archive.FileName)
                    ? archiveId + ".zip"
                    : request.Archive.FileName)
        );

        var entity = RoadNetworkChangesArchive.Upload(archiveId);

        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            var problems = entity.ValidateArchiveUsing(archive, _validator);

            var fileProblems = problems.OfType<FileError>();
            if (fileProblems.Any())
            {
                var translatedProblems = problems.Select(problem => problem.Translate()).ToArray();
                throw new ValidationException(translatedProblems.Select(s => new ValidationFailure(s.File, $"{s.Reason} - {s.File}")));
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

            await _sqsQueuePublisher.CopyToQueue(SqsQueueName.FeatureCompare.RequestQueue, message, new SqsQueueOptions { MessageGroupId = SqsFeatureCompare.MessageGroupId }, cancellationToken);
        }

        return new UploadExtractFeatureCompareResponse(archiveId);
    }

    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };
}