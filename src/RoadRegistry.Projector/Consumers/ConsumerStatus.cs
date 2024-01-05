using System;

namespace RoadRegistry.Projector.Consumers;

public class ConsumerStatus
{
    public string Name { get; set; }
    public DateTimeOffset LastProcessedMessage { get; set; }
}
