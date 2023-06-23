namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;

public class ChangeLaneAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Aantal rijstroken van het wegsegment.
    /// </summary>
    [DataMember(Name = "Aantal", Order = 3)]
    [JsonProperty("aantal", Required = Required.Always)]
    public int? Aantal { get; set; }

    /// <summary>
    ///     Richting t.o.v. de richting van het wegsegment (begin- naar
    ///     eindknoop).
    /// </summary>
    [DataMember(Name = "Richting", Order = 4)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentLaneDirection))]
    public string Richting { get; set; }
}
