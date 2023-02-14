namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.BlobStore;

public abstract class BlobMessageHandler : IMessageHandler
{
    private readonly SqsMessagesBlobClient _blobClient;

    protected BlobMessageHandler(SqsMessagesBlobClient blobClient)
    {
        _blobClient = blobClient;
    }

    protected abstract Task HandleMessageAsync(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken);

    public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
    {
        if (messageData is BlobRequest blobRequest)
        {
            var blobName = new BlobName(blobRequest.BlobName);

            messageMetadata.Logger?.LogInformation($"Downloading blob '{blobName}'");
            var blobMessageData = await _blobClient.GetBlobMessageAsync(blobName, cancellationToken);
            messageMetadata.Logger?.LogInformation($"Downloaded blob '{blobName}'");

            await HandleMessageDataAsync(blobMessageData, messageMetadata, cancellationToken);

            messageMetadata.Logger?.LogInformation($"Deleting blob '{blobName}'");
            await _blobClient.DeleteBlobAsync(blobName, cancellationToken);
            messageMetadata.Logger?.LogInformation($"Deleted blob '{blobName}'");
        }
        else
        {
            await HandleMessageDataAsync(messageData, messageMetadata, cancellationToken);
        }
    }

    private async Task HandleMessageDataAsync(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
    {
        messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");
        await HandleMessageAsync(messageData, messageMetadata, cancellationToken);
        messageMetadata.Logger?.LogInformation($"Handled message {messageData?.GetType().Name}");
    }
}
