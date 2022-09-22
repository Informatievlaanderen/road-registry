using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.Tests.BackOffice
{
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Messages;
    using Xunit;

    public class FakeSqsTests
    {
        private readonly SqsQueueOptions _sqsQueueOptions;
        private readonly CancellationToken _cancellationToken;

        public FakeSqsTests()
        {
            _sqsQueueOptions = new SqsQueueOptions();
            _cancellationToken = CancellationToken.None;
        }

        [Fact]
        public async Task Fake_sqs_should_publish_and_consume()
        {
            var queueUrl = "road-registry/test-queue";
            var queueName = SqsQueue.ParseQueueNameFromQueueUrl(queueUrl);

            var originalMessage = new UploadRoadNetworkChangesArchive() { ArchiveId = new ArchiveId(Guid.NewGuid().ToString("N")).ToString() };

            var sqsPublisher = new FakeSqsQueuePublisher();
            await sqsPublisher.CopyToQueue(queueName, originalMessage, _sqsQueueOptions, _cancellationToken);

            var sqsConsumer = new FakeSqsQueueConsumer();
            await sqsConsumer.Consume(queueUrl, message =>
            {
                var uploadRoadNetworkChangesArchive = Assert.IsType<UploadRoadNetworkChangesArchive>(message);
                Assert.Equal(originalMessage.ArchiveId, uploadRoadNetworkChangesArchive.ArchiveId);

                return Task.CompletedTask;
            }, _cancellationToken);
        }
    }
}
