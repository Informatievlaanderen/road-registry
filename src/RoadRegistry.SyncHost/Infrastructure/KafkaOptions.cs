namespace RoadRegistry.SyncHost.Infrastructure;

using BackOffice;

public class KafkaOptions : IHasConfigurationKey
{
    public string BootstrapServers { get; private set; }
    public string SaslUserName { get; private set; }
    public string SaslPassword { get; private set; }
    public KafkaConsumers Consumers { get; private set; }

    public string GetConfigurationKey()
    {
        return "Kafka";
    }
}

public class KafkaConsumers
{
    public KafkaConsumer StreetNameEvent { get; private set; }
    public KafkaConsumer StreetNameSnapshot { get; private set; }
    public KafkaConsumer MunicipalityEvent { get; private set; }
}

public class KafkaConsumer
{
    public string Topic { get; private set; }
    public string GroupSuffix { get; private set; }
    public string JsonPath { get; private set; }
}
