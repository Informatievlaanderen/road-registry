using System.Reflection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Uploads;
using SqsQueue = Be.Vlaanderen.Basisregisters.Sqs.SqsQueue;

public class S3SqsQueue : ISqsQueue
{
    private readonly SqsQueue _sqsQueue;
    private readonly RoadNetworkSqsMessagesBlobClient _blobClient;

    public S3SqsQueue(SqsOptions sqsOptions, SqsQueueUrlOptions sqsQueueUrlOptions, RoadNetworkSqsMessagesBlobClient blobClient)
    {
        _sqsQueue = new SqsQueue(sqsOptions, sqsQueueUrlOptions.SqsQueueUrl);
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
