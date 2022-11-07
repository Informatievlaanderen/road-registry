namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare.Fixtures;

using Abstractions.Configuration;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.MessagingHost.Sqs.Consumers;

public abstract class WhenMessageReceivedFixture : IAsyncLifetime
{
    private readonly FeatureCompareMessageConsumer _backgroundService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly SqsQueueOptions _sqsQueueOptions;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;

    protected WhenMessageReceivedFixture(IMediator mediator, ISqsQueuePublisher sqsQueuePublisher, ISqsQueueConsumer sqsQueueConsumer, SqsQueueOptions sqsQueueOptions, ILoggerFactory loggerFactory)
    {
        _sqsQueuePublisher = sqsQueuePublisher;
        _sqsQueueOptions = sqsQueueOptions;

        _messagingOptions = new FeatureCompareMessagingOptions
        {
            RequestQueueUrl = "request.fifo",
            ResponseQueueUrl = "response.fifo"
        };

        _backgroundService = new FeatureCompareMessageConsumer(
            mediator,
            loggerFactory.CreateLogger<FeatureCompareMessageConsumer>(),
            _messagingOptions,
            sqsQueueConsumer
        );
        _cancellationTokenSource = new CancellationTokenSource();
    }

    protected abstract object[] MessageRequestCollection { get; }
    public bool Result { get; set; }

    public async Task DisposeAsync()
    {
        await _backgroundService.StopAsync(_cancellationTokenSource.Token);
    }

    public async Task InitializeAsync()
    {
        foreach (var message in MessageRequestCollection)
        {
            var sqsQueueName = SqsQueue.ParseQueueNameFromQueueUrl(_messagingOptions.ResponseQueueUrl);
            await _sqsQueuePublisher.CopyToQueue(sqsQueueName, message, _sqsQueueOptions, _cancellationTokenSource.Token);
        }

        await _backgroundService.StartAsync(_cancellationTokenSource.Token);
    }
}
