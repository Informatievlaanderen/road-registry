namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions;
using Amazon.Lambda.Core;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Commands;
using Ductus.FluentDocker.Extensions;

public class CheckFeatureCompareDockerContainerCommandHandler : LambdaCommandHandler<CheckFeatureCompareDockerContainerCommand, CheckFeatureCompareDockerContainerCommandResponse>
{
    private readonly FeatureCompareMessagingOptions _messagingOptions;
    private readonly ISqsQueueConsumer _sqsQueueConsumer;
    private readonly ISqsQueuePublisher _sqsQueuePublisher;

    public CheckFeatureCompareDockerContainerCommandHandler(
        FeatureCompareMessagingOptions messagingOptions,
        ISqsQueuePublisher sqsQueuePublisher,
        ISqsQueueConsumer sqsQueueConsumer)
    {
        _messagingOptions = messagingOptions;
        _sqsQueuePublisher = sqsQueuePublisher;
        _sqsQueueConsumer = sqsQueueConsumer;
    }

    public override async Task<CheckFeatureCompareDockerContainerCommandResponse> HandleAsync(CheckFeatureCompareDockerContainerCommand request, ILambdaContext context, CancellationToken cancellationToken)
    {
        var container = FeatureCompareDockerContainerBuilder.Default.Build();

        CheckFeatureCompareDockerContainerCommandResponse response;
        using (container)
        {
            var serviceState = container.GetConfiguration(true).State.ToServiceState();
            response = new CheckFeatureCompareDockerContainerCommandResponse(serviceState);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            context.Logger.LogInformation("Received cancellation request. Exit without failure. See you on the next timer run!");
            await Task.FromCanceled<CheckFeatureCompareDockerContainerCommandResponse>(cancellationToken);
        }

        if (response.IsRunning)
        {
            context.Logger.LogInformation("Feature compare container found. Exit without failure. See you on the next timer run!");
            await Task.FromCanceled<CheckFeatureCompareDockerContainerCommandResponse>(cancellationToken);
        }
        else
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var consumerCounter = 0;

            await _sqsQueueConsumer.Consume(_messagingOptions.RequestQueueUrl, async message =>
            {
                if (consumerCounter >= 1)
                    throw new IndexOutOfRangeException("Consumer within Lambda MUST only process one single message per invocation!");

                // Publish message from one queue to another
                await _sqsQueuePublisher.CopyToQueue(_messagingOptions.DockerQueueName, message, new SqsQueueOptions(), cancellationToken);

                // Cancel the cancellation token so we don't get stuck inside the consumer loop
                cancellationTokenSource.Cancel();
                consumerCounter++;
            }, cancellationTokenSource.Token);
        }

        return response;
    }
}

public class FeatureCompareMessagingOptions
{
    public string DockerQueueName => SqsQueue.ParseQueueNameFromQueueUrl(DockerQueueUrl);
    public string DockerQueueUrl { get; set; }
    public string RequestQueueName => SqsQueue.ParseQueueNameFromQueueUrl(RequestQueueUrl);
    public string RequestQueueUrl { get; set; }
    public string ResponseQueueName => SqsQueue.ParseQueueNameFromQueueUrl(ResponseQueueUrl);
    public string ResponseQueueUrl { get; set; }
}