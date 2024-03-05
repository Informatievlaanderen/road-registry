namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using Infrastructure.Controllers.Attributes;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentAantalRijstrokenWijzigen", Namespace = "")]
public class ChangeLaneAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Aantal rijstroken van het wegsegment (geheel getal tussen 1 en 10 of "niet gekend" of "niet van toepassing").
    /// </summary>
    [DataMember(Name = "Aantal", Order = 3)]
    [JsonProperty("aantal", Required = Required.Always)]
    public string Aantal { get; set; }

    /// <summary>
    ///     Richting t.o.v. de richting van het wegsegment (begin- naar
    ///     eindknoop).
    /// </summary>
    [DataMember(Name = "Richting", Order = 4)]
    [JsonProperty("richting", Required = Required.Always)]
    [RoadRegistryEnumDataType(typeof(RoadSegmentLaneDirection))]
    public string Richting { get; set; }
}
