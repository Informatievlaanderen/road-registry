namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

public interface ISqsQueuePublisher
{
    T GetLatestMessage<T>();
    Task<bool> CopyToQueue<T>(string queueName, T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class;
}
