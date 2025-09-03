namespace RoadRegistry.Tests.BackOffice;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Handlers.Sqs;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;

public class FakeSqsTests
{
    private readonly SqsQueueOptions _sqsQueueOptions;

    public FakeSqsTests()
    {
        _sqsQueueOptions = new SqsQueueOptions();
    }

    [Fact]
    public async Task Fake_sqs_should_publish_and_consume()
    {
        var queueUrl = "road-registry/test-queue.fifo";
        var cancellationTokenSource = new CancellationTokenSource();

        var originalMessage = new UploadRoadNetworkChangesArchive { ArchiveId = new ArchiveId(Guid.NewGuid().ToString("N")).ToString() };

        var sqsQueueFactoryAndConsumer = new SqsQueueFactoryAndConsumerForDevelopment(new SqsJsonMessageSerializer(new SqsOptions()), new NullLoggerFactory());
        sqsQueueFactoryAndConsumer.RemoveQueue(queueUrl);

        var sqsPublisher = sqsQueueFactoryAndConsumer.Create(queueUrl);
        await sqsPublisher.Copy(originalMessage, _sqsQueueOptions, cancellationTokenSource.Token);

        try
        {
            await sqsQueueFactoryAndConsumer.Consume(queueUrl, message =>
            {
                var uploadRoadNetworkChangesArchive = Assert.IsType<UploadRoadNetworkChangesArchive>(message);
                Assert.Equal(originalMessage.ArchiveId, uploadRoadNetworkChangesArchive.ArchiveId);

                cancellationTokenSource.Cancel();
                return Task.CompletedTask;
            }, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // do nothing
        }
    }
}
