namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Shared
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer _producer;

        public KafkaProducer(ProducerOptions producerOptions)
        {
            _producer = new Producer(producerOptions);
        }

        public Task<Result> Produce<T>(int objectId, T message, CancellationToken cancellationToken)
            where T : class, IQueueMessage
        {
            var key = objectId.ToString(CultureInfo.InvariantCulture);

            return _producer.ProduceJsonMessage(
                new MessageKey(key),
                message,
                [],
                cancellationToken);
        }
    }
}
