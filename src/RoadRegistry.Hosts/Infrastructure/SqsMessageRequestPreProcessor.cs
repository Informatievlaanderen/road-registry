namespace RoadRegistry.Hosts.Infrastructure;

using System.Threading;
using System.Threading.Tasks;
using BackOffice.Abstractions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

public class SqsMessageRequestPreProcessor : IRequestPreProcessor<SqsMessageRequest>
{
    private readonly SqsMessagesBlobClient _blobClient;
    private readonly ILogger<SqsMessageRequestPreProcessor> _logger;

    public SqsMessageRequestPreProcessor(SqsMessagesBlobClient blobClient, ILogger<SqsMessageRequestPreProcessor> logger)
    {
        _blobClient = blobClient;
        _logger = logger;
    }

    public async Task Process(SqsMessageRequest request, CancellationToken cancellationToken)
    {
        if (request.Data is BlobRequest blobRequest)
        {
            request.Data = await _blobClient.GetBlobMessageAsync(new BlobName(blobRequest.BlobName), cancellationToken);
        }

        if (request.Data is not SqsRequest sqsRequest)
        {
            var ex = new SqsMessageException($"Unable to cast '{nameof(request.Data)}' as {nameof(SqsRequest)}.");
            _logger.LogInformation(ex.Message);
            throw ex;
        }
    }
}
