namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

public interface ISqsQueueConsumer
{
    public Task Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken);
}
