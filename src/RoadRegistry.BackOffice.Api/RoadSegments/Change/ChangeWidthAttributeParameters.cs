namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System.Runtime.Serialization;
using Newtonsoft.Json;

public class ChangeWidthAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Breedte van het wegsegment(in meter).
    /// </summary>
    [DataMember(Name = "Breedte", Order = 3)]
    [JsonProperty("breedte", Required = Required.Always)]
    public int? Breedte { get; set; }
}
