namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using System.IO.Compression;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Framework;
using Microsoft.Extensions.Logging;
using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

/// <summary>Upload controller, post upload</summary>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
public class UploadExtractFeatureCompareRequestHandler : EndpointRequestHandler<UploadExtractFeatureCompareRequest, UploadExtractResponse>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly SqsOptions _sqsOptions;
    private readonly IZipArchiveValidator _validator;

    public UploadExtractFeatureCompareRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        SqsOptions sqsOptions,
        IZipArchiveValidator validator,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new UploadExtractBlobClientNotFoundException(nameof(client));
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _validator = validator ?? throw new ValidatorNotFoundException(nameof(validator));
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
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

        await _client.CreateBlobAsync(
            new BlobName(archiveId.ToString()),
            metadata,
            ContentType.Parse("application/zip"),
            readStream,
            cancellationToken
        );

        var message = RoadNetworkChangesArchive.Upload(archiveId);

        readStream.Position = 0;
        using (var archive = new ZipArchive(readStream, ZipArchiveMode.Read, false))
        {
            message.ValidateArchiveUsing(archive, _validator);
        }

        await CopyToQueue(_sqsOptions, SqsQueueName.Value, message, new SqsQueueOptions { MessageGroupId = archiveId }, cancellationToken);

        return new UploadExtractResponse(archiveId);
    }
}
