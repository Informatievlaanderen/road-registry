namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;

    public interface IKafkaProducer
    {
        Task<Result<T>> Produce<T>(string key, T message, CancellationToken cancellationToken)
            where T : class, IQueueMessage;
    }
}
