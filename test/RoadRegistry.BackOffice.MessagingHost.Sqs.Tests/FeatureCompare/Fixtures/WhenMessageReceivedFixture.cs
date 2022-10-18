namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare.Fixtures;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Configuration;
using MediatR;
using Microsoft.Extensions.Logging;

public abstract class WhenMessageReceivedFixture : IAsyncLifetime
{
    protected WhenMessageReceivedFixture(IMediator mediator, ISqsQueuePublisher sqsQueuePublisher, ISqsQueueConsumer sqsQueueConsumer, SqsQueueOptions sqsQueueOptions, ILoggerFactory loggerFactory)
    {
        _sqsQueuePublisher = sqsQueuePublisher;
        _sqsQueueOptions = sqsQueueOptions;

        _messagingOptions = new FeatureCompareMessagingOptions
        {
            RequestQueueUrl = "request.fifo",
            ResponseQueueUrl = "response.fifo"
        };

        _backgroundService = new FeatureCompareMessageResponseConsumer(
            mediator,
            _messagingOptions,
            sqsQueueConsumer,
            loggerFactory.CreateLogger<FeatureCompareMessageResponseConsumer>()
        );
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private readonly FeatureCompareMessageResponseConsumer _backgroundService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly SqsQueueOptions _sqsQueueOptions;

    private readonly ISqsQueuePublisher _sqsQueuePublisher;

    public async Task DisposeAsync()
    {
        await _backgroundService.StopAsync(_cancellationTokenSource.Token);
    }

    public async Task InitializeAsync()
    {
        foreach (var message in MessageRequestCollection)
            await _sqsQueuePublisher.CopyToQueue(_messagingOptions.ResponseQueueName, message, _sqsQueueOptions, _cancellationTokenSource.Token);

        await _backgroundService.StartAsync(_cancellationTokenSource.Token);
    }

    protected abstract object[] MessageRequestCollection { get; }

    public bool Result { get; set; }
}
