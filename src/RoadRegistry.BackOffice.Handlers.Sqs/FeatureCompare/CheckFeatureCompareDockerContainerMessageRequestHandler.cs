namespace RoadRegistry.BackOffice.Handlers.Sqs.FeatureCompare
{
    using Abstractions;
    using Abstractions.Configuration;
    using Abstractions.FeatureCompare;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Ductus.FluentDocker.Extensions;
    using Microsoft.Extensions.Logging;
    using Polly;
    using RoadRegistry.BackOffice.Infrastructure;

    public class CheckFeatureCompareDockerContainerMessageRequestHandler : SqsMessageRequestHandler<CheckFeatureCompareDockerContainerMessageRequest, CheckFeatureCompareDockerContainerMessageResponse>
    {
        private readonly FeatureCompareMessagingOptions _messagingOptions;
        private readonly ISqsQueueConsumer _sqsQueueConsumer;
        private readonly ISqsQueuePublisher _sqsQueuePublisher;

        public CheckFeatureCompareDockerContainerMessageRequestHandler(
            FeatureCompareMessagingOptions messagingOptions,
            ISqsQueuePublisher sqsQueuePublisher,
            ISqsQueueConsumer sqsQueueConsumer,
            ILogger<CheckFeatureCompareDockerContainerMessageRequestHandler> logger)
            : base(null, logger)
        {
            _messagingOptions = messagingOptions;
            _sqsQueuePublisher = sqsQueuePublisher;
            _sqsQueueConsumer = sqsQueueConsumer;
        }

        public override async Task<CheckFeatureCompareDockerContainerMessageResponse> HandleAsync(CheckFeatureCompareDockerContainerMessageRequest request, CancellationToken cancellationToken)
        {
            var container = FeatureCompareDockerContainerBuilder.Default.Build();

            CheckFeatureCompareDockerContainerMessageResponse response;
            using (container)
            {
                var serviceState = container.GetConfiguration(true).State.ToServiceState();
                response = new CheckFeatureCompareDockerContainerMessageResponse(serviceState);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Received cancellation request. Exit without failure. See you on the next timer run!");
                await Task.FromCanceled<CheckFeatureCompareDockerContainerMessageResponse>(cancellationToken);
            }

            if (response.IsRunning)
            {
                _logger.LogInformation("Container {container} state: {containerState}", container.Name, response.State);
                _logger.LogInformation("Exit without failure. See you on the next timer run!");

                await Task.FromCanceled<CheckFeatureCompareDockerContainerMessageResponse>(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Container {container} state: {containerState}", container.Name, response.State);

                var cancellationTokenSource = new CancellationTokenSource();
                var consumerCounter = 0;

                await _sqsQueueConsumer.Consume(_messagingOptions.RequestQueueUrl, async message =>
                {
                    _logger.LogInformation("Attempting to dequeue message from queue: {requestQueueUrl}", _messagingOptions.RequestQueueUrl);

                    if (consumerCounter >= 1)
                        throw new IndexOutOfRangeException("Consumer within Lambda MUST only process one single message per invocation!");

                    // Publish message from one queue to another
                    var sqsQueueName = SqsQueue.ParseQueueNameFromQueueUrl(_messagingOptions.DockerQueueUrl);

                    _logger.LogInformation("Attempting to publish message onto queue: {sqsQueueName}", sqsQueueName);
                    await _sqsQueuePublisher.CopyToQueue(sqsQueueName, message, new SqsQueueOptions(), cancellationToken);

                    // Cancel the cancellation token so we don't get stuck inside the consumer loop
                    cancellationTokenSource.Cancel();
                    consumerCounter++;
                }, cancellationTokenSource.Token);
            }

            return response;

        }
    }
}
