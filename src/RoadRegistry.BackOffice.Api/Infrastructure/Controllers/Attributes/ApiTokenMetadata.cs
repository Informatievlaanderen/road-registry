namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using Newtonsoft.Json;

public record ApiTokenMetadata([JsonProperty("wraccess")] bool WrAccess, [JsonProperty("syncaccess")] bool SyncAccess = false, [JsonProperty("ticketsaccess")] bool TicketsAccess = false);