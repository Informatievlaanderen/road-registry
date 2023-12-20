namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Consumers;

using Abstractions.Configuration;
using Exceptions;
using MediatR;
using Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class FeatureCompareMessageConsumer : BackgroundService
{
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public FeatureCompareMessageConsumer(
        IMediator mediator,
        ILogger<FeatureCompareMessageConsumer> logger,
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueueConsumer sqsQueueConsumer
    )
    {
        _mediator = mediator;
        _messagingOptions = messagingOptions;
        _sqsConsumer = sqsQueueConsumer;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _sqsConsumer.Consume(_messagingOptions.ResponseQueueUrl, async message =>
                {
                    _logger.LogInformation("SQS message from feature compare received!");
                    await HandleMessageAsync(message, stoppingToken);
                }, stoppingToken);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogTrace(ex, "Task has been cancelled, wait until next run...");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"An unhandled exception has occurred! {ex.Message}");
            }
            finally
            {
                await Task.Delay(_messagingOptions.ConsumerDelaySeconds * 1000, stoppingToken);
            }
        }
    }

    public async Task HandleMessageAsync(object message, CancellationToken stoppingToken)
    {
        switch (message)
        {
            case UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive:
                {
                    _logger.LogInformation("Received message for archive {ArchiveId}", uploadRoadNetworkChangesArchive.ArchiveId);
                    var request = new Abstractions.Uploads.FeatureCompare.FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                    await _mediator.Send(request, stoppingToken);
                }
                break;
            case UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive:
                {
                    _logger.LogInformation("Received message for archive {ArchiveId}", uploadRoadNetworkExtractChangesArchive.ArchiveId);
                    var request = new Abstractions.Extracts.FeatureCompare.FeatureCompareProcessOutputMessageRequest(
                        uploadRoadNetworkExtractChangesArchive.ArchiveId,
                        uploadRoadNetworkExtractChangesArchive.DownloadId,
                        uploadRoadNetworkExtractChangesArchive.RequestId,
                        uploadRoadNetworkExtractChangesArchive.UploadId
                    );
                    await _mediator.Send(request, stoppingToken);
                }
                break;
            default:
                throw new UnknownSqsMessageTypeException($"Unhandled message type '{message.GetType()}' found on queue '{_messagingOptions.ResponseQueueUrl}'", message.GetType().FullName);
        }
    }
}
