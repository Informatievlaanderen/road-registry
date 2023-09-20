namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare.Fixtures;

using Abstractions.Configuration;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using MediatR;
using Messages;
using Microsoft.Extensions.Logging;

public class WhenMessageReceivedWithKnownTypeFixture : WhenMessageReceivedFixture
{
    public WhenMessageReceivedWithKnownTypeFixture(IMediator mediator, ISqsQueuePublisher sqsQueuePublisher, ISqsQueueConsumer sqsQueueConsumer, SqsQueueOptions sqsQueueOptions, FeatureCompareMessagingOptions messagingOptions, ILoggerFactory loggerFactory)
        : base(mediator, sqsQueuePublisher, sqsQueueConsumer, sqsQueueOptions, messagingOptions, loggerFactory)
    {
    }

    protected override object[] MessageRequestCollection => new object[]
    {
        new UploadRoadNetworkChangesArchive
        {
            ArchiveId = "b9279138f87f416497365ee0cc935d11"
        }
    };
}