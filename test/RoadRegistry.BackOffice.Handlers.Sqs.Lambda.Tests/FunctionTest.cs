using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Messages;
using Newtonsoft.Json;
using Uploads;

public class FunctionTest
{
    public FunctionTest()
    {
    }

    [Fact]
    public async Task When_uploading_archive_message_received()
    {
        if (ArchiveId.Accepts("14b6dc1668c949a3a2761f7f545ee768"))
        {
            var archiveId = new ArchiveId("14b6dc1668c949a3a2761f7f545ee768");
            var message = RoadNetworkChangesArchive.Upload(archiveId);

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = message.Id.ToString()
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }
    }

    [Fact]
    public async Task When_uploading_archive_extract_message_received()
    {
        var message = new UploadRoadNetworkExtractChangesArchive
        {
            ArchiveId = "14b6dc1668c949a3a2761f7f545ee768",
            DownloadId = Guid.Empty,
            RequestId = Guid.Empty.ToString(),
            UploadId = Guid.Empty,
        };

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = message.ArchiveId
                }
            }
        };

        var logger = new TestLambdaLogger();
        var context = new TestLambdaContext
        {
            Logger = logger
        };

        var function = new Function();
        await function.FunctionHandler(sqsEvent, context);

        Assert.Contains("Processed message foobar", logger.Buffer.ToString());
    }
}
