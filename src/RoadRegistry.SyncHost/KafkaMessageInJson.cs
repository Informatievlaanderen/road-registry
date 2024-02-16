namespace RoadRegistry.SyncHost;

using System.Collections.Generic;

public class KafkaMessageInJson
{
    public Dictionary<string, string> Headers { get; set; }
    public string Value { get; set; }
    public string Key { get; set; }
    public long Offset { get; set; }
}
