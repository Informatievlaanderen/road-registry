namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Newtonsoft.Json;

    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaProducerOptions _kafkaProducerOptions;

        public KafkaProducer(KafkaProducerOptions kafkaProducerOptions)
        {
            _kafkaProducerOptions = kafkaProducerOptions;
        }

        public async Task<Result> Produce<T>(string key, T message, CancellationToken cancellationToken) where T : class, IQueueMessage
        {
            var kafkaMessage = new
            {
                Type = message.GetType().FullName!,
                Data = message
            };
            var kafkaJsonMessage = JsonConvert.SerializeObject(kafkaMessage, Formatting.Indented, _kafkaProducerOptions.JsonSerializerSettings);
            return await Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple.KafkaProducer.Produce(_kafkaProducerOptions, key, kafkaJsonMessage, cancellationToken);
        }
    }
}
