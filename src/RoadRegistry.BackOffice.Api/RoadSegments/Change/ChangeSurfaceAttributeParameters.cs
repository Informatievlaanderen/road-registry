namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

public class ChangeSurfaceAttributeParameters : ChangePositionAttributeParameters
{
    /// <summary>
    ///     Type wegverharding van het wegsegment.
    /// </summary>
    [DataMember(Name = "Type", Order = 3)]
    [JsonProperty("type", Required = Required.Always)]
    [EnumDataType(typeof(RoadSegmentSurfaceType))]
    public string Type { get; set; }
}
