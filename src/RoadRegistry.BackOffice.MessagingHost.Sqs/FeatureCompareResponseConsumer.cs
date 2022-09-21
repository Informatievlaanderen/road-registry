namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Configuration;
using Exceptions;
using Framework;
using MediatR;
using Messages;
using Newtonsoft.Json;
using Requests;

public class FeatureCompareResponseConsumer : BackgroundService
{
    private readonly ILogger<FeatureCompareResponseConsumer> _logger;
    private readonly IMediator _mediator;
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;
    private readonly JsonSerializerSettings _serializerSettings;

    public FeatureCompareResponseConsumer(
        IMediator mediator,
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueueConsumer sqsQueueConsumer,
        ILogger<FeatureCompareResponseConsumer> logger)
    {
        _mediator = mediator;
        _messagingOptions = messagingOptions;
        _sqsConsumer = sqsQueueConsumer;
        _logger = logger;

        _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await _sqsConsumer.Consume(_messagingOptions.QueueUrl, async (message) =>
            {
                var messageWrapper = (SimpleQueueCommand)message;
                var actualMessage = messageWrapper.ToActualType(_serializerSettings);

                switch (actualMessage)
                {
                    case UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive:
                        var request = new FeatureCompareMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                        await _mediator.Send(request, cancellationToken);
                        break;
                    default:
                        throw new UnknownSqsMessageTypeException($"Unhandled message type '{messageWrapper.Type}' found on queue '{_messagingOptions.QueueUrl}'", messageWrapper.Type);
                }
            }, cancellationToken);

            await Task.Delay(1000, cancellationToken);
        }
    }
}
