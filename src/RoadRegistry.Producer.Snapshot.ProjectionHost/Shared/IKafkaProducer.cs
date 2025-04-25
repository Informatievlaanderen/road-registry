namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Shared
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;

    public interface IKafkaProducer
    {
        Task<Result> Produce<T>(int objectId, T message, long storePosition, CancellationToken cancellationToken)
            where T : class, IQueueMessage;
    }
}
