namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System.Runtime.Serialization;
using Newtonsoft.Json;

public abstract class ChangePositionAttributeParameters
{
    /// <summary>
    ///     Van positie.
    /// </summary>
    [DataMember(Name = "VanPositie", Order = 1)]
    [JsonProperty("vanPositie", Required = Required.Always)]
    public decimal? VanPositie { get; set; }

    /// <summary>
    ///     Tot positie.
    /// </summary>
    [DataMember(Name = "TotPositie", Order = 2)]
    [JsonProperty("totPositie", Required = Required.Always)]
    public decimal? TotPositie { get; set; }
}
