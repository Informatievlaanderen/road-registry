namespace RoadRegistry.StreetName;
using Newtonsoft.Json;

public class StreetNameWasRemovedV2
{
    [JsonProperty("persistentLocalId")]
    public int PersistentLocalId { get; set; }
}

public class StreetNameWasRenamed
{
    [JsonProperty("persistentLocalId")]
    public int PersistentLocalId { get; set; }

    [JsonProperty("destinationPersistentLocalId")]
    public int DestinationPersistentLocalId { get; set; }
}
