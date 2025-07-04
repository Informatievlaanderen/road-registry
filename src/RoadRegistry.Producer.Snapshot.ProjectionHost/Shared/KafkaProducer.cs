namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Shared
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Newtonsoft.Json;

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer _producer;
        private readonly JsonSerializerSettings _serializerSettings;

        public KafkaProducer(ProducerOptions producerOptions)
        {
            _producer = new Producer(producerOptions);
            _serializerSettings = producerOptions.JsonSerializerSettings;
        }

        public Task<Result> Produce<T>(int objectId, T message, CancellationToken cancellationToken)
            where T : class, IQueueMessage
        {
            var kafkaMessage = new
            {
                Type = message.GetType().FullName!,
                Data = message
            };
            var kafkaJsonMessage = JsonConvert.SerializeObject(kafkaMessage, Formatting.Indented, _serializerSettings);

            var key = objectId.ToString(CultureInfo.InvariantCulture);

            return _producer.Produce(
                new MessageKey(key),
                kafkaJsonMessage,
                [],
                cancellationToken);
        }
    }
}
