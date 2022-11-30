namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;

    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaProducerOptions _kafkaProducerOptions;

        public KafkaProducer(KafkaProducerOptions kafkaProducerOptions)
        {
            _kafkaProducerOptions = kafkaProducerOptions;
        }

        public Task<Result<T>> Produce<T>(string key, T message, CancellationToken cancellationToken) where T : class, IQueueMessage
            => Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple.KafkaProducer.Produce(_kafkaProducerOptions, key, message, cancellationToken);
    }
}
