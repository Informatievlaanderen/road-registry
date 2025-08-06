namespace RoadRegistry.Tests.BackOffice;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;

public class FakeSqsTests
{
    private readonly CancellationToken _cancellationToken;
    private readonly SqsQueueOptions _sqsQueueOptions;

    public FakeSqsTests()
    {
        _sqsQueueOptions = new SqsQueueOptions();
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task Fake_sqs_should_publish_and_consume()
    {
        var queueUrl = "road-registry/test-queue";

        var originalMessage = new UploadRoadNetworkChangesArchive { ArchiveId = new ArchiveId(Guid.NewGuid().ToString("N")).ToString() };

        var sqsPublisher = new FakeSqsQueuePublisher();
        await sqsPublisher.CopyToQueue(queueUrl, originalMessage, _sqsQueueOptions, _cancellationToken);

        var sqsConsumer = new FakeSqsQueueConsumer(new SqsJsonMessageSerializer(new SqsOptions()), new NullLoggerFactory());
        await sqsConsumer.Consume(queueUrl, message =>
        {
            var uploadRoadNetworkChangesArchive = Assert.IsType<UploadRoadNetworkChangesArchive>(message);
            Assert.Equal(originalMessage.ArchiveId, uploadRoadNetworkChangesArchive.ArchiveId);

            return Task.CompletedTask;
        }, _cancellationToken);
    }
}
