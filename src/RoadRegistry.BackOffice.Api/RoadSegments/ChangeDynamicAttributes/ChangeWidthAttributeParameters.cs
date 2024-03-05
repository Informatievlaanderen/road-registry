namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Infrastructure;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentWegbreedteWijzigen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentWegbreedteWijzigen")]
public class ChangeWidthAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Breedte van het wegsegment in meter (geheel getal tussen 1 en 50 of "niet gekend" of "niet van toepassing").
    /// </summary>
    [DataMember(Name = "Breedte", Order = 3)]
    [JsonProperty("breedte", Required = Required.Always)]
    public string Breedte { get; set; }
}
