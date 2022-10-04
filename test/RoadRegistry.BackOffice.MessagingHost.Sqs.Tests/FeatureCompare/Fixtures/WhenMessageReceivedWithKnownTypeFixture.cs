namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare.Fixtures;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using MediatR;
using Messages;
using Microsoft.Extensions.Logging;

public class WhenMessageReceivedWithKnownTypeFixture : WhenMessageReceivedFixture
{
    public WhenMessageReceivedWithKnownTypeFixture(IMediator mediator, ISqsQueuePublisher sqsQueuePublisher, ISqsQueueConsumer sqsQueueConsumer, SqsQueueOptions sqsQueueOptions, ILoggerFactory loggerFactory)
        : base(mediator, sqsQueuePublisher, sqsQueueConsumer, sqsQueueOptions, loggerFactory)
    {
    }

    protected override object[] MessageRequestCollection => new object[]
    {
        new UploadRoadNetworkChangesArchive
        {
            ArchiveId =  "b9279138f87f416497365ee0cc935d11"
        }
    };
}
