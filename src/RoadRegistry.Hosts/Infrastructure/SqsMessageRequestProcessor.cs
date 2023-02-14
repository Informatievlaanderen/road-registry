namespace RoadRegistry.Hosts.Infrastructure;

using System.Threading;
using System.Threading.Tasks;
using BackOffice.Abstractions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

public class SqsMessageRequestProcessor :
    IRequestPreProcessor<SqsMessageRequest>,
    IRequestPostProcessor<SqsMessageRequest, SqsMessageResponse>
{
    private readonly SqsMessagesBlobClient _blobClient;
    private readonly ILogger<SqsMessageRequestProcessor> _logger;

    public SqsMessageRequestProcessor(SqsMessagesBlobClient blobClient, ILogger<SqsMessageRequestProcessor> logger)
    {
        _blobClient = blobClient;
        _logger = logger;
    }

    public async Task Process(SqsMessageRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Post processor started for message {request.Data?.GetType().Name}");

        if (request.Data is BlobRequest blobRequest)
        {
            var blobName = new BlobName(blobRequest.BlobName);
            request.Data = await _blobClient.GetBlobMessageAsync(blobName, cancellationToken);
            _logger.LogInformation("SQS message downloaded from S3 ({BlobName})", blobName);
        }

        if (request.Data is not SqsRequest sqsRequest)
        {
            var ex = new SqsMessageException($"Unable to cast '{nameof(request.Data)}' as {nameof(SqsRequest)}.");
            _logger.LogError($"Post processor failed for message {request.Data?.GetType().Name}", ex);
            throw ex;
        }

        _logger.LogError($"Post processor finished for message {request.Data?.GetType().Name}");
    }

    public async Task Process(SqsMessageRequest request, SqsMessageResponse response, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Post processor started for message {request.Data?.GetType().Name}");

        if (request.Data is BlobRequest blobRequest)
        {
            var blobName = new BlobName(blobRequest.BlobName);
            await _blobClient.DeleteBlobAsync(blobName, cancellationToken);

            _logger.LogInformation("SQS message stored on S3 deleted ({BlobName})", blobName);
        }

        _logger.LogInformation($"Post processor finished for message {request.Data?.GetType().Name}");
    }
}
