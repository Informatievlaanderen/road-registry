namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Configuration;
using Abstractions.FeatureCompare;
using Exceptions;
using Infrastructure;
using MediatR;
using Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

public class FeatureCompareMessageConsumer : ApplicationBackgroundService
{
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;

    public FeatureCompareMessageConsumer(
        IMediator mediator,
        ILogger<FeatureCompareMessageConsumer> logger,
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueueConsumer sqsQueueConsumer
    )
        : base(mediator, logger, messagingOptions.ConsumerDelaySeconds)
    {
        _messagingOptions = messagingOptions;
        _sqsConsumer = sqsQueueConsumer;
    }

    protected override async Task ExecuteCallbackAsync(CancellationToken cancellationToken)
    {
        await _sqsConsumer.Consume(_messagingOptions.ResponseQueueUrl, async message =>
        {
            Logger.LogInformation("Feature compare started for new message received from SQS");

            switch (message)
            {
                case UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive:
                {
                    Logger.LogInformation("Received message for archive {ArchiveId}", uploadRoadNetworkChangesArchive.ArchiveId);
                    var request = new FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                    await Mediator.Send(request, cancellationToken);
                }
                    break;
                case UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive:
                {
                    Logger.LogInformation("Received message for archive {ArchiveId}", uploadRoadNetworkExtractChangesArchive.ArchiveId);
                    var request = new FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkExtractChangesArchive.ArchiveId);
                    await Mediator.Send(request, cancellationToken);
                }
                    break;
                case JObject jObject:
                {
                    var uploadRoadNetworkChangesArchive = jObject.ToObject<UploadRoadNetworkChangesArchive>();

                    Logger.LogInformation("Received message for archive {ArchiveId}", uploadRoadNetworkChangesArchive.ArchiveId);
                    var request = new FeatureCompareProcessOutputMessageRequest(uploadRoadNetworkChangesArchive.ArchiveId);
                    await Mediator.Send(request, cancellationToken);
                }
                    break;

                default:
                    throw new UnknownSqsMessageTypeException($"Unhandled message type '{message.GetType()}' found on queue '{_messagingOptions.ResponseQueueUrl}'", message.GetType().FullName);
            }
        }, cancellationToken);
    }
}
