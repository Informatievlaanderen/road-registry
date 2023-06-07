using System.Reflection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Uploads;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public interface IBackOfficeS3SqsQueue : ISqsQueue
{
}

internal class BackOfficeS3SqsQueue : IBackOfficeS3SqsQueue
{
    private readonly SqsQueue _sqsQueue;
    private readonly SqsOptions _sqsOptions;
    private readonly SqsMessagesBlobClient _blobClient;
    private readonly ILogger<BackOfficeS3SqsQueue> _logger;

    public BackOfficeS3SqsQueue(SqsOptions sqsOptions, SqsQueueUrlOptions sqsQueueUrlOptions, SqsMessagesBlobClient blobClient, ILogger<BackOfficeS3SqsQueue> logger)
    {
        _sqsQueue = new SqsQueue(sqsOptions, sqsQueueUrlOptions.BackOffice);
        _sqsOptions = sqsOptions;
        _blobClient = blobClient;
        _logger = logger;
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

        var serializer = JsonSerializer.CreateDefault(_sqsOptions.JsonSerializerSettings);
        var sqsJsonMessage = SqsJsonMessage.Create(message, serializer);
        var json = serializer.Serialize(sqsJsonMessage);
        
        _logger.LogInformation("Sqs message:\n{Message}", json);
    }
}
