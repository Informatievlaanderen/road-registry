namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Uploads;
using System.Reflection;
using RoadRegistry.Extensions;

public interface IBackOfficeS3SqsQueue : ISqsQueue
{
}

internal class BackOfficeS3SqsQueue : IBackOfficeS3SqsQueue
{
    private readonly ISqsQueue _sqsQueue;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly SqsMessagesBlobClient _blobClient;
    private readonly ILogger<BackOfficeS3SqsQueue> _logger;

    public BackOfficeS3SqsQueue(ISqsQueueFactory sqsQueueFactory, SqsJsonMessageSerializer sqsJsonMessageSerializer, SqsQueueUrlOptions sqsQueueUrlOptions, SqsMessagesBlobClient blobClient, ILogger<BackOfficeS3SqsQueue> logger)
    {
        _sqsQueue = sqsQueueFactory.ThrowIfNull().Create(sqsQueueUrlOptions.BackOffice);
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
        _blobClient = blobClient.ThrowIfNull(); ;
        _logger = logger.ThrowIfNull(); ;
    }

    public async Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        LogMessage(message);

        if (typeof(T).GetCustomAttribute(typeof(BlobRequestAttribute)) != null)
        {
            var blobName = new BlobName($"{Guid.NewGuid():N}.sqs");

            await _blobClient.CreateBlobMessageAsync(
                blobName,
                Metadata.None,
                ContentType.Parse("text/json"),
                message,
                cancellationToken
            );

            var blobRequest = new BlobRequest
            {
                BlobName = blobName
            };
            return await _sqsQueue.Copy(blobRequest, queueOptions, cancellationToken);
        }

        return await _sqsQueue.Copy(message, queueOptions, cancellationToken);
    }

    private void LogMessage<T>(T message)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        _logger.LogInformation("Sqs message:\n{Message}", _sqsJsonMessageSerializer.Serialize(message));
    }
}
