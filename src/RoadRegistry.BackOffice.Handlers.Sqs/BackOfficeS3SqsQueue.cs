using System.Reflection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Uploads;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

namespace RoadRegistry.BackOffice.Handlers.Sqs;

public class BackOfficeS3SqsQueue : ISqsQueue
{
    private readonly SqsQueue _sqsQueue;
    private readonly SqsMessagesBlobClient _blobClient;

    public BackOfficeS3SqsQueue(SqsOptions sqsOptions, SqsQueueUrlOptions sqsQueueUrlOptions, SqsMessagesBlobClient blobClient)
    {
        _sqsQueue = new SqsQueue(sqsOptions, sqsQueueUrlOptions.BackOffice);
        _blobClient = blobClient;
    }

    public async Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
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
}
