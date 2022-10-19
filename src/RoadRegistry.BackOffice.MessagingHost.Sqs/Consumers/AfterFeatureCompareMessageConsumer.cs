namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Consumers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Configuration;
using Abstractions.FeatureCompare;
using Exceptions;
using MediatR;
using Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;

public class AfterFeatureCompareMessageConsumer : BackgroundService
{
    private readonly ILogger<AfterFeatureCompareMessageConsumer> _logger;
    private readonly IMediator _mediator;
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;

    public AfterFeatureCompareMessageConsumer(
        IMediator mediator,
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueueConsumer sqsQueueConsumer,
        ILogger<AfterFeatureCompareMessageConsumer> logger)
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

            await _sqsConsumer.Consume(_messagingOptions.ResponseQueueUrl, async message =>
            {
                switch (message)
                {
                    case UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive:
                        {
                            var request = new FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                            await _mediator.Send(request, cancellationToken);
                        }
                        break;
                    case UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive:
                        {
                            var request = new FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkExtractChangesArchive.ArchiveId);
                            await _mediator.Send(request, cancellationToken);
                        }
                        break;
                    default:
                        throw new UnknownSqsMessageTypeException($"Unhandled message type '{message.GetType()}' found on queue '{_messagingOptions.ResponseQueueUrl}'", message.GetType().FullName);
                }
            }, cancellationToken);

            await Task.Delay(_messagingOptions.ConsumerDelaySeconds * 1000, cancellationToken);
        }
    }
}
