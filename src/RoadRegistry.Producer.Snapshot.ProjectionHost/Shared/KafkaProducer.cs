namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Newtonsoft.Json;

    public class KafkaProducer : IKafkaProducer
    {
        private readonly ProducerOptions _producerOptions;
        private readonly IProducer _producer;

        public KafkaProducer(ProducerOptions producerOptions)
        {
            _producerOptions = producerOptions;
            _producer = new Producer(producerOptions);
        }

        public Task<Result> Produce<T>(int objectId, T message, long storePosition, CancellationToken cancellationToken) where T : class, IQueueMessage
        {
            //TODO-pr moet dat nog steeds in deze structuur? conversie van de simplekafka
            var kafkaMessage = new
            {
                Type = message.GetType().FullName!,
                Data = message
            };

            var kafkaJsonMessage = JsonConvert.SerializeObject(kafkaMessage, Formatting.Indented, _producerOptions.JsonSerializerSettings);
            var key = objectId.ToString(CultureInfo.InvariantCulture);

            //TODO-pr idempotencykey toegevoegd, ok?
            return _producer.Produce(
                new MessageKey(key),
                kafkaJsonMessage,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition}") },
                cancellationToken);
        }
    }
}
