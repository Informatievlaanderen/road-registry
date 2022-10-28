namespace RoadRegistry.BackOffice.Handlers.Kafka;

public class ConsumerOptions
{
    public ConsumerOptions(string topic, string consumerGroupSuffix)
    {
        Topic = topic;
        ConsumerGroupSuffix = consumerGroupSuffix;
    }

    public string Topic { get; }
    public string ConsumerGroupSuffix { get; }
}
