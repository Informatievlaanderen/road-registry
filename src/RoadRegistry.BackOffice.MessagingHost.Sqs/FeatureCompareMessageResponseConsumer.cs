namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using Configuration;
using Exceptions;
using MediatR;
using Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.FeatureCompare;

public class FeatureCompareMessageResponseConsumer : BackgroundService
{
    private readonly ILogger<FeatureCompareMessageResponseConsumer> _logger;
    private readonly IMediator _mediator;
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;

    public FeatureCompareMessageResponseConsumer(
        IMediator mediator,
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueueConsumer sqsQueueConsumer,
        ILogger<FeatureCompareMessageResponseConsumer> logger)
    {
        _mediator = mediator;
        _messagingOptions = messagingOptions;
        _sqsConsumer = sqsQueueConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await _sqsConsumer.Consume(_messagingOptions.ResponseQueueUrl, async (message) =>
            {
                switch (message)
                {
                    case UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive:
                        {
                            var request = new FeatureCompareMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                            await _mediator.Send(request, cancellationToken);
                        }
                        break;
                    case UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive:
                        {
                            var request = new FeatureCompareMessageRequest(uploadRoadNetworkExtractChangesArchive.ArchiveId);
                            await _mediator.Send(request, cancellationToken);
                        }
                        break;
                    default:
                        throw new UnknownSqsMessageTypeException($"Unhandled message type '{message.GetType()}' found on queue '{_messagingOptions.ResponseQueueName}'", message.GetType().FullName);
                }
            }, cancellationToken);

            await Task.Delay(_messagingOptions.ConsumerDelaySeconds * 1000, cancellationToken);
        }
    }
}
