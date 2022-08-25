namespace RoadRegistry.BackOffice.Api.Tests.Framework;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging;

public class FakeSqsQueueConsumer : SqsQueueConsumer
{
    public FakeSqsQueueConsumer(SqsOptions sqsOptions, ILogger<SqsQueueConsumer> logger) : base(sqsOptions, logger)
    {
    }
}
